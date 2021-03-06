﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Simpleloginsystem.Models
{
    public class UserContext : DbContext
    {
        public DbSet<Register> Register { get; set; }
        public DbSet<Login> login { get; set; }
        public DbSet<ForgotPassword> forgot { get; set; }
        public DbSet<ChangePassword> ChangeRole { get; set; }
    }
}