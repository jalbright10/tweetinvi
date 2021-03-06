﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Core.Interfaces.Credentials;
using Tweetinvi.Core.Interfaces.DTO;
using Tweetinvi.Core.Interfaces.QueryGenerators;
using Tweetinvi.Factories.Properties;
using Tweetinvi.Logic.DTO;

namespace Tweetinvi.Factories.User
{
    public interface IUserFactoryQueryExecutor
    {
        IUserDTO GetLoggedUser();

        IUserDTO GetUserDTOFromId(long userId);
        IUserDTO GetUserDTOFromScreenName(string userName);

        List<IUserDTO> GetUsersDTOFromIds(IEnumerable<long> userIds);
        List<IUserDTO> GetUsersDTOFromScreenNames(IEnumerable<string> userScreenNames);

        List<IUserDTO> LookupUserIds(List<long> userIds);
        List<IUserDTO> LookupUserScreenNames(List<string> userName);
    }

    public class UserFactoryQueryExecutor : IUserFactoryQueryExecutor
    {
        private const int MAX_LOOKUP_USERS = 100;

        private readonly ITwitterAccessor _twitterAccessor;
        private readonly IUserQueryParameterGenerator _queryParameterGenerator;

        public UserFactoryQueryExecutor(ITwitterAccessor twitterAccessor, IUserQueryParameterGenerator queryParameterGenerator)
        {
            _twitterAccessor = twitterAccessor;
            _queryParameterGenerator = queryParameterGenerator;
        }

        // Get single user
        public IUserDTO GetLoggedUser()
        {
            var query = Resources.TokenUser_GetCurrentUser;
            return _twitterAccessor.ExecuteGETQuery<IUserDTO>(query);
        }

        public IUserDTO GetUserDTOFromId(long userId)
        {
            var query = string.Format(Resources.User_GetUserFromId, userId);
            return _twitterAccessor.ExecuteGETQuery<IUserDTO>(query);
        }

        public IUserDTO GetUserDTOFromScreenName(string userName)
        {
            var query = string.Format(Resources.User_GetUserFromName, userName);
            return _twitterAccessor.ExecuteGETQuery<IUserDTO>(query);
        }

        // Get Multiple users
        public List<IUserDTO> GetUsersDTOFromIds(IEnumerable<long> userIds)
        {
            var usersDTO = new List<IUserDTO>();

            for (int i = 0; i < userIds.Count(); i += MAX_LOOKUP_USERS)
            {
                var userIdsToLookup = userIds.Skip(i).Take(MAX_LOOKUP_USERS).ToList();
                var retrievedUsers = LookupUserIds(userIdsToLookup);
                usersDTO.AddRangeSafely(retrievedUsers);

                if (retrievedUsers == null)
                {
                    break;
                }
            }

            return usersDTO;
        }

        public List<IUserDTO> GetUsersDTOFromScreenNames(IEnumerable<string> userScreenNames)
        {
            var usersDTO = new List<IUserDTO>();

            for (int i = 0; i < userScreenNames.Count(); i += MAX_LOOKUP_USERS)
            {
                var userScreenNamesToLookup = userScreenNames.Skip(i).Take(MAX_LOOKUP_USERS).ToList();
                var retrievedUsers = LookupUserScreenNames(userScreenNamesToLookup);
                usersDTO.AddRangeSafely(retrievedUsers);

                if (retrievedUsers == null)
                {
                    break;
                }
            }

            return usersDTO;
        }

        // Lookup
        public List<IUserDTO> LookupUserIds(List<long> userIds)
        {
            if (userIds.Count > MAX_LOOKUP_USERS)
            {
                throw new InvalidOperationException("Cannot retrieve that quantity of users at once");
            }

            var userIdsParameter = _queryParameterGenerator.GenerateListOfIdsParameter(userIds);
            var query = string.Format(Resources.User_GetUsersFromIds, userIdsParameter);

            return _twitterAccessor.ExecutePOSTQuery<List<IUserDTO>>(query);
        }

        public List<IUserDTO> LookupUserScreenNames(List<string> userName)
        {
            if (userName.Count > MAX_LOOKUP_USERS)
            {
                throw new InvalidOperationException("Cannot retrieve that quantity of users at once");
            }

            var userIdsParameter = _queryParameterGenerator.GenerateListOfScreenNameParameter(userName);
            var query = string.Format(Resources.User_GetUsersFromNames, userIdsParameter);

            return _twitterAccessor.ExecutePOSTQuery<List<IUserDTO>>(query);
        }
    }
}