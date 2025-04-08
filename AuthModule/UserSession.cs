using System;
using System.Collections.Generic;

namespace AuthModule
{
    public class UserSession
    {
        private static UserSession _instance;
        private static readonly object _lock = new object();
        private bool _isInitialized = false;

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

        private UserSession() { }

        public string UserType { get; private set; }
        public HashSet<string> Permissions { get; private set; }
        public bool IsLoggedIn => _isInitialized;

        public void Initialize(string userType, HashSet<string> permissions)
        {
            UserType = userType;
            Permissions = new HashSet<string>(permissions);
            _isInitialized = true;
        }

        public void Clear()
        {
            UserType = null;
            Permissions = null;
            _isInitialized = false;
        }

        public bool HasPermission(string permission)
        {
            if (!IsLoggedIn || Permissions == null) return false;
            return Permissions.Contains(permission);
        }

        public bool HasAnyPermission(IEnumerable<string> permissions)
        {
            if (!IsLoggedIn || Permissions == null) return false;
            foreach (var permission in permissions)
            {
                if (Permissions.Contains(permission)) return true;
            }
            return false;
        }

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