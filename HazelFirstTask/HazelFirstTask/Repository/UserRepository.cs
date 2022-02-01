using AutoMapper;
using BCryptNet = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Linq;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Repository;
using System;

namespace WebApi.Services
{
    public class UserRepository : IUserRepository
    {
        private DataContext _context;
        private IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;

        public UserRepository(
            DataContext context,
            IJwtUtils jwtUtils,
            IMapper mapper)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _mapper = mapper;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var response = new AuthenticateResponse();
            response.Username = model.Username;
            response.Email = model.Email;

            var CheckUserByUsername = _context.Users.SingleOrDefault(x => x.Username == model.Username);
            var CheckUserByEmail = _context.Users.SingleOrDefault(x => x.Email == model.Email);

            // validate
            if ((CheckUserByUsername == null && CheckUserByEmail==null) || 
                (!BCryptNet.Verify(model.Password, CheckUserByUsername.PasswordHash)
                && !BCryptNet.Verify(model.Password, CheckUserByEmail.PasswordHash)))
            {
            response.Token = "Token Not Generated UserName or Password Incorect !";
                return response;
            }

            // authentication successful
            if (CheckUserByUsername != null)
            {
                response = _mapper.Map<AuthenticateResponse>(CheckUserByUsername);
                response.Token = _jwtUtils.GenerateToken(CheckUserByUsername);
                return response;
            }
            else if(CheckUserByEmail != null)
            {
                response = _mapper.Map<AuthenticateResponse>(CheckUserByEmail);
                response.Token = _jwtUtils.GenerateToken(CheckUserByEmail);
                return response;
            }
            response.Token = "Token Not Generated UserName or Password Incorect !";
            return response;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public User GetById(int id)
        {
            return getUser(id);
        }

        public void Register(RegisterRequest model)
        {
            // validate
            if (_context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            // map model to new user object
            var user = _mapper.Map<User>(model);

            // hash password
            user.PasswordHash = BCryptNet.HashPassword(model.Password);

            // save user
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(int id, UpdateRequest model)
        {
            var user = getUser(id);

            // validate
            if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = BCryptNet.HashPassword(model.Password);

            // copy model to user and save
            _mapper.Map(model, user);
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = getUser(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        // helper methods

        private User getUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) throw new KeyNotFoundException("User not found");
            return user;
        }
    }
}