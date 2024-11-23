namespace UnityEngine
{
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            if (!component.TryGetComponent<T>(out var attachedComponent))
            {
                attachedComponent = component.gameObject.AddComponent<T>();
            }
            return attachedComponent;
        }
    }
}
