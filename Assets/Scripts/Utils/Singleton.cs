using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 泛型单例模式基类，用于Unity中需要单例功能的MonoBehaviour组件
/// </summary>
/// <typeparam name="T">必须是继承自MonoBehaviour的类型</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 静态私有字段，存储单例实例
    private static T _instance;

    // 公开的静态属性，提供全局访问点
    // 使用表达式体属性简化语法
    public static T Instance => _instance;

    /// <summary>
    /// Unity生命周期方法，在对象初始化时调用
    /// 用于确保单例的唯一性
    /// </summary>
    protected virtual void Awake()
    {
        // 检查是否已存在实例且不是当前对象
        if (_instance != null && _instance != this)
        {
            // 如果已存在实例，销毁当前对象以保持单例
            Destroy(this.gameObject);
        }
        else
        {
            // 如果没有实例，将当前对象设为单例实例
            _instance = this as T;
        }
    }
}

