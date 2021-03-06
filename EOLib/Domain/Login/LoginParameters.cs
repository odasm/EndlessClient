﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Login
{
    public class LoginParameters : ILoginParameters
    {
        public string Username { get; }
        public string Password { get; }

        public LoginParameters(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    public interface ILoginParameters
    {
        string Username { get; }
        string Password { get; }
    }
}