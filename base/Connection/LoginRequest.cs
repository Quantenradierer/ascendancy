﻿using System;

namespace @base.connection
{
    public class LoginRequest : Request
    {
        public LoginRequest(Core.Models.Position position, string username, string password)
            : base(Guid.Empty, position)
        {
            Username = username;
            Password = password;
        }

        public string Username;
        public string Password;
    }
}

