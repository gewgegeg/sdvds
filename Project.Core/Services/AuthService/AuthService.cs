﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monit.Project.DAL.Entities;
using Monit.Project.DAL.Interfaces;

namespace Monit.Project.Core.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly IDbRepository _dbRepository;

        public AuthService(IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }

        /// <exception cref="KeyNotFoundException"></exception>
        /// /// <exception cref="ArgumentException"></exception>
        public async Task<Session> OpenSessionAsync(string userName, string hash)
        {
            var user = await _dbRepository.Get<User>(usr => usr.Name == userName).FirstOrDefaultAsync();
            if (user == null)
                throw new KeyNotFoundException();
            if (user.Hash != hash)
                throw new ArgumentException();


            var session = new Session()
            {
                Token = Guid.NewGuid().ToString(), // change to jwtbearer
                ExpireAt = DateTime.Now.AddMinutes(10)
            };

            await _dbRepository.Add(session);
            await _dbRepository.SaveChangesAsync();

            return session;
        }

        /// <exception cref="KeyNotFoundException"></exception>
        public async Task CloseSessionAsync(string token)
        {
            var session = await _dbRepository.Get<Session>(ses => ses.Token == token).FirstOrDefaultAsync();
            if (session == null)
                throw new KeyNotFoundException();

            await _dbRepository.Remove(session);
            await _dbRepository.SaveChangesAsync();
        }

        /// <exception cref="ArgumentException"></exception>
        public async Task RegisterNewUserAsync(string userName, string email, string hash)
        {
            var user = await _dbRepository.Get<User>(usr => usr.Name == userName).FirstOrDefaultAsync();
            if (user != null)
                throw new ArgumentException();

            var newUser = new User
            {
                Name = userName,
                Email = email,
                Hash = hash
            };

            await _dbRepository.Add(newUser);
            await _dbRepository.SaveChangesAsync();
        }

        public async Task<bool> IsSessionOpenAsync(string token)
        {
            var session = await _dbRepository.Get<Session>(ses => ses.Token == token).FirstOrDefaultAsync();
            if (session == null)
                return false;

            var isExpired = session.ExpireAt < DateTime.Now;

            if (isExpired)
                await CloseSessionAsync(session.Token);

            return !isExpired;
        }
    }
}
