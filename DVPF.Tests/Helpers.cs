using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DVPF.Tests
{
    internal static class Helpers
    {
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="R">The return type of method "methodName".</typeparam>
        ///// <param name="targetClass"></param>
        ///// <param name="methodName"></param>
        ///// <param name="methodArgs"></param>
        ///// <returns></returns>
        //internal static R InvokePrivateMethod<R>(object targetClass, string methodName, params object[] methodArgs)
        //{
        //    PrivateObject obj = new PrivateObject(targetClass);

        //    return (R)obj.Invoke(methodName, methodArgs);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="targetClass"></param>
        ///// <param name="methodName"></param>
        ///// <param name="methodArgs"></param>
        ///// <returns></returns>
        //internal static void InvokePrivateMethod(object targetClass, string methodName, params object[] methodArgs)
        //{
        //    PrivateObject obj = new PrivateObject(targetClass);

        //    obj.Invoke(methodName, methodArgs);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="R">The return type of method "methodName".</typeparam>
        ///// <typeparam name="C">The generic class containing the "methodName" method.</typeparam>
        ///// <param name="methodName"></param>
        ///// <param name="methodArgs"></param>
        ///// <returns></returns>
        //internal static R InvokePrivateStaticMethod<R, C>(string methodName, params object[] methodArgs)
        //{
        //    PrivateType pt = new PrivateType(typeof(C));

        //    return (R)pt.InvokeStatic(methodName, methodArgs);
        //}

        //internal static R GetPrivateStaticProperty<R, C>(string propertyName)
        //{
        //    PrivateType pt = new PrivateType(typeof(C));

        //    return (R)pt.GetStaticProperty(propertyName);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="C">The generic class containing the "methodName" method.</typeparam>
        ///// <param name="targetClass"></param>
        ///// <param name="methodName"></param>
        ///// <param name="methodArgs"></param>
        ///// <returns></returns>
        //internal static void InvokePrivateStaticMethod<C>(string methodName, params object[] methodArgs)
        //{
        //    PrivateType pt = new PrivateType(typeof(C));

        //    pt.InvokeStatic(methodName, methodArgs);
        //}

  
    }
}
