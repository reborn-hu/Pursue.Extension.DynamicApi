using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pursue.Extension.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pursue.Extension.DynamicApi
{
    sealed class AutoApiConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var type = controller.ControllerType.AsType();
                var autoApiAttribute = ReflectionExtensions.GetSingleAttributeOrDefaultByFullSearch<AutoApiAttribute>(type.GetTypeInfo());

                if (typeof(IEnableAutoApi).GetTypeInfo().IsAssignableFrom(type))
                {
                    controller.ControllerName = controller.ControllerName.RemovePostFix(AutoApiOptions.RemoveControllerPostfixes.ToArray());
                    ConfigureArea(controller, autoApiAttribute);
                    ConfigureDynamicWebApi(controller, autoApiAttribute);
                }
                else
                {
                    if (autoApiAttribute != null)
                    {
                        ConfigureArea(controller, autoApiAttribute);
                        ConfigureDynamicWebApi(controller, autoApiAttribute);
                    }
                }
            }
        }

        private static void ConfigureArea(ControllerModel controller, AutoApiAttribute autoApiAttribute)
        {
            if (!controller.RouteValues.ContainsKey("area"))
            {
                if (!string.IsNullOrEmpty(autoApiAttribute.Version))
                {
                    controller.RouteValues["area"] = autoApiAttribute.Version;
                }
                else if (!string.IsNullOrEmpty(AutoApiOptions.DefaultVersion))
                {
                    controller.RouteValues["area"] = AutoApiOptions.DefaultVersion;
                }
            }
        }

        private static void ConfigureDynamicWebApi(ControllerModel controller, AutoApiAttribute controllerAttr)
        {
            ConfigureApiExplorer(controller);
            ConfigureSelector(controller, controllerAttr);
            ConfigureParameters(controller);
        }

        #region ConfigureApiExplorer

        private static void ConfigureApiExplorer(ControllerModel controller)
        {
            if (controller.ApiExplorer.GroupName.IsNullOrEmpty())
            {
                controller.ApiExplorer.GroupName = controller.ControllerName;
            }
            if (controller.ApiExplorer.IsVisible == null)
            {
                controller.ApiExplorer.IsVisible = true;
            }
            foreach (var action in controller.Actions)
            {
                if (action.ApiExplorer.IsVisible == null)
                {
                    action.ApiExplorer.IsVisible = true;
                }
            }
        }

        #endregion

        #region ConfigureSelector

        private static void ConfigureSelector(ControllerModel controller, AutoApiAttribute controllerAttr)
        {
            RemoveEmptySelectors(controller.Selectors);

            if (controller.Selectors.Any(selector => selector.AttributeRouteModel != null))
            {
                return;
            }

            var areaName = string.Empty;

            if (controllerAttr != null)
            {
                areaName = controllerAttr.Version;
            }

            var controllerName = controller.ControllerName;

            foreach (var action in controller.Actions)
            {
                RemoveEmptySelectors(action.Selectors);

                var nonAttr = ReflectionExtensions.GetSingleAttributeOrDefault<DisableApiAttribute>(action.ActionMethod);
                if (nonAttr != null)
                {
                    return;
                }
                if (!action.Selectors.Any())
                {
                    AddAppServiceSelector(areaName, controllerName, action);
                }
                else
                {
                    NormalizeSelectorRoutes(areaName, controllerName, action);
                }
            }
        }

        #endregion

        #region ConfigureParameters

        private static void ConfigureParameters(ControllerModel controller)
        {
            foreach (var action in controller.Actions)
            {
                foreach (var para in action.Parameters)
                {
                    if (para.BindingInfo != null)
                    {
                        continue;
                    }

                    if (!TypeExtensions.IsPrimitiveExtendedIncludingNullable(para.ParameterInfo.ParameterType))
                    {
                        if (CanUseFormBodyBinding(action, para))
                        {
                            para.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromBodyAttribute() });
                        }
                    }
                }
            }
        }

        #endregion

        private static bool CanUseFormBodyBinding(ActionModel action, ParameterModel parameter)
        {
            if (AutoApiOptions.FormBodyBindingIgnoredTypes.Any(t => t.IsAssignableFrom(parameter.ParameterInfo.ParameterType)))
            {
                return false;
            }

            foreach (var selector in action.Selectors)
            {
                if (selector.ActionConstraints == null)
                {
                    continue;
                }

                foreach (var actionConstraint in selector.ActionConstraints)
                {
                    if (actionConstraint is HttpMethodActionConstraint httpMethodActionConstraint)
                    {
                        if (httpMethodActionConstraint.HttpMethods.All(hm => hm.IsIn("GET", "DELETE", "TRACE", "HEAD")))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static void AddAppServiceSelector(string areaName, string controllerName, ActionModel action)
        {
            var verbKey = action.ActionName.GetPascalOrCamelCaseFirstWord().ToLower();
            var verb = AutoApiOptions.HttpVerbs.TryGetValue(verbKey, out string value) ? value : AutoApiOptions.DefaultHttpVerb;

            action.ActionName = GetRestFulActionName(action.ActionName);

            var appServiceSelectorModel = new SelectorModel
            {
                AttributeRouteModel = CreateActionRouteModel(areaName, controllerName, action.ActionName)
            };

            appServiceSelectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { verb }));

            action.Selectors.Add(appServiceSelectorModel);
        }

        private static string GetRestFulActionName(string actionName)
        {
            // 删除后缀
            actionName = actionName.RemovePostFix(AutoApiOptions.RemoveActionPostfixes.ToArray());

            // 删除前缀
            var verbKey = actionName.GetPascalOrCamelCaseFirstWord().ToLower();

            if (AutoApiOptions.HttpVerbs.ContainsKey(verbKey))
            {
                if (actionName.Length == verbKey.Length)
                    return "";
                else
                    return actionName[verbKey.Length..];
            }
            else
                return actionName;
        }

        private static void NormalizeSelectorRoutes(string areaName, string controllerName, ActionModel action)
        {
            action.ActionName = GetRestFulActionName(action.ActionName);
            foreach (var selector in action.Selectors)
            {
                selector.AttributeRouteModel = selector.AttributeRouteModel == null ? CreateActionRouteModel(areaName, controllerName, action.ActionName) : AttributeRouteModel.CombineAttributeRouteModel(CreateActionRouteModel(areaName, controllerName, ""), selector.AttributeRouteModel);
            }
        }

        private static AttributeRouteModel CreateActionRouteModel(string areaName, string controllerName, string actionName)
        {
            return new AttributeRouteModel(new RouteAttribute($"{AutoApiOptions.DefaultApiPreFix}/{areaName}/{controllerName}/{actionName}".Replace("//", "/")));
        }

        private static void RemoveEmptySelectors(IList<SelectorModel> selectors)
        {
            selectors.Where(IsEmptySelector).ToList().ForEach(s => selectors.Remove(s));
        }

        private static bool IsEmptySelector(SelectorModel selector)
        {
            return selector.AttributeRouteModel == null && selector.ActionConstraints.IsNullOrEmpty();
        }
    }
}