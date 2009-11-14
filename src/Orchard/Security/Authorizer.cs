﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Security.Permissions;
using Orchard.UI.Notify;

namespace Orchard.Security {
    public interface IAuthorizer : IDependency {
        bool Authorize(Permission permission, LocalizedString message);
    }

    public class Authorizer : IAuthorizer {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;

        public Authorizer(
            IAuthorizationService authorizationService,
            INotifier notifier) {
            _authorizationService = authorizationService;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public IUser CurrentUser { get; set; }
        public Localizer T { get; set; }

        public bool Authorize(Permission permission, LocalizedString message) {
            if (_authorizationService.CheckAccess(CurrentUser, permission))
                return true;

            if (CurrentUser == null) {
                _notifier.Error(T("{0}. Anonymous users do not have {1} permission.",
                                  message, permission.Name));
            }
            else {
                _notifier.Error(T("{0}. Current user, {2}, does not have {1} permission.",
                                  message, permission.Name, CurrentUser.UserName));
            }
            return false;
        }
    }
}