﻿using NUnit.Framework;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ObjectMapperTest
    {
        private readonly User _sourceUser = new User
        {
            Email = "geovanni.perez@unosquare.com",
            Name = "Geo",
            Role = new Role { Name = "Admin" }
        };

        [Test]
        public void SimpleMapTest()
        {
            Runtime.ObjectMapper.CreateMap<User, UserDto>();

            var destination = Runtime.ObjectMapper.Map<UserDto>(_sourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(_sourceUser.Name, destination.Name);
            Assert.AreEqual(_sourceUser.Email, destination.Email);
            Assert.IsNull(destination.Role);
        }

        [Test]
        public void PropertyMapTest()
        {
            Runtime.ObjectMapper.CreateMap<User, UserDto>().MapProperty(t => t.Role, s => s.Role.Name);

            var destination = Runtime.ObjectMapper.Map<UserDto>(_sourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(_sourceUser.Name, destination.Name);
            Assert.AreEqual(_sourceUser.Email, destination.Email);
            Assert.AreEqual(_sourceUser.Role.Name, destination.Name);
        }

        [Test]
        public void AutoMapTest()
        {
            var mapper = new ObjectMapper();
            var destination = mapper.Map<UserDto>(_sourceUser);

            Assert.IsNotNull(destination);
            Assert.AreEqual(_sourceUser.Name, destination.Name);
            Assert.AreEqual(_sourceUser.Email, destination.Email);
            Assert.IsNull(destination.Role);
        }
    }
}
