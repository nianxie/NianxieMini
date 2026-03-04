using UnityEngine;

namespace Nianxie.Bridge
{
    public class UnityEngineInternalBridge
    {
        public static void RegisterProperty(Object driver, Object target, string propertyPath)
        {
            DrivenPropertyManager.TryRegisterProperty(driver, target, propertyPath);
        }

        public static void UnregisterProperty(Object driver, Object target, string propertyPath)
        {
            DrivenPropertyManager.UnregisterProperty(driver, target, propertyPath);
        }

        public static void UnregisterProperties(Object driver)
        {
            DrivenPropertyManager.UnregisterProperties(driver);
        }
    }
}
