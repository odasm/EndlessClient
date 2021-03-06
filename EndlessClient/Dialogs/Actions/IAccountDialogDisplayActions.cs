﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EOLib;
using EOLib.Domain.Account;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    public interface IAccountDialogDisplayActions
    {
        void ShowInitialCreateWarningDialog();

        Task<XNADialogResult> ShowCreatePendingDialog();

        Task<Optional<IChangePasswordParameters>> ShowChangePasswordDialog();

        void ShowCreateParameterValidationError(CreateAccountParameterResult validationResult);
        
        void ShowCreateAccountServerError(AccountReply serverError);
        
        void ShowSuccessMessage();
    }
}
