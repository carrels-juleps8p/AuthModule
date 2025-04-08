using System;
using System.Collections.Generic;

namespace AuthModule
{
    /// <summary>
    /// 用户会话管理类，使用单例模式确保整个应用程序只有一个用户会话实例
    /// 负责管理用户的登录状态、用户类型和权限信息
    /// </summary>
    public class UserSession
    {
        // 单例实例
        private static UserSession _instance;
        // 线程锁对象，用于确保线程安全
        private static readonly object _lock = new object();
        // 标记是否已初始化
        private bool _isInitialized = false;

        /// <summary>
        /// 获取UserSession的单例实例
        /// 使用双重检查锁定模式确保线程安全
        /// </summary>
        public static UserSession Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UserSession();
                        }
                    }
                }
                return _instance;
            }
        }

        // 私有构造函数，防止外部直接实例化
        private UserSession() { }

        /// <summary>
        /// 获取当前用户类型（操作员/管理员/超级管理员）
        /// </summary>
        public string UserType { get; private set; }

        /// <summary>
        /// 获取当前用户的所有权限集合
        /// </summary>
        public HashSet<string> Permissions { get; private set; }

        /// <summary>
        /// 获取用户是否已登录
        /// </summary>
        public bool IsLoggedIn => _isInitialized;

        /// <summary>
        /// 初始化用户会话
        /// </summary>
        /// <param name="userType">用户类型</param>
        /// <param name="permissions">用户权限集合</param>
        public void Initialize(string userType, HashSet<string> permissions)
        {
            UserType = userType;
            Permissions = new HashSet<string>(permissions);
            _isInitialized = true;
        }

        /// <summary>
        /// 清除用户会话信息，用于用户登出
        /// </summary>
        public void Clear()
        {
            UserType = null;
            Permissions = null;
            _isInitialized = false;
        }

        /// <summary>
        /// 检查用户是否具有指定权限
        /// </summary>
        /// <param name="permission">要检查的权限</param>
        /// <returns>如果用户具有指定权限则返回true，否则返回false</returns>
        public bool HasPermission(string permission)
        {
            if (!IsLoggedIn || Permissions == null) return false;
            return Permissions.Contains(permission);
        }

        /// <summary>
        /// 检查用户是否具有指定权限集合中的任意一个权限
        /// </summary>
        /// <param name="permissions">要检查的权限集合</param>
        /// <returns>如果用户具有任意一个指定权限则返回true，否则返回false</returns>
        public bool HasAnyPermission(IEnumerable<string> permissions)
        {
            if (!IsLoggedIn || Permissions == null) return false;
            foreach (var permission in permissions)
            {
                if (Permissions.Contains(permission)) return true;
            }
            return false;
        }

        /// <summary>
        /// 检查用户是否同时具有指定权限集合中的所有权限
        /// </summary>
        /// <param name="permissions">要检查的权限集合</param>
        /// <returns>如果用户具有所有指定权限则返回true，否则返回false</returns>
        public bool HasAllPermissions(IEnumerable<string> permissions)
        {
            if (!IsLoggedIn || Permissions == null) return false;
            foreach (var permission in permissions)
            {
                if (!Permissions.Contains(permission)) return false;
            }
            return true;
        }
    }
} 