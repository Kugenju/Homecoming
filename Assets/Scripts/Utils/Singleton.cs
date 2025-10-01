using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ���͵���ģʽ���࣬����Unity����Ҫ�������ܵ�MonoBehaviour���
/// </summary>
/// <typeparam name="T">�����Ǽ̳���MonoBehaviour������</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // ��̬˽���ֶΣ��洢����ʵ��
    private static T _instance;

    // �����ľ�̬���ԣ��ṩȫ�ַ��ʵ�
    // ʹ�ñ��ʽ�����Լ��﷨
    public static T Instance => _instance;

    /// <summary>
    /// Unity�������ڷ������ڶ����ʼ��ʱ����
    /// ����ȷ��������Ψһ��
    /// </summary>
    protected virtual void Awake()
    {
        // ����Ƿ��Ѵ���ʵ���Ҳ��ǵ�ǰ����
        if (_instance != null && _instance != this)
        {
            // ����Ѵ���ʵ�������ٵ�ǰ�����Ա��ֵ���
            Destroy(this.gameObject);
        }
        else
        {
            // ���û��ʵ��������ǰ������Ϊ����ʵ��
            _instance = this as T;
        }
    }
}

