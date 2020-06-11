using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using System.Security;
using System.Activities.Statements;
using System.ComponentModel;
using UiPathTeam.OrchestratorAPI.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;

namespace UiPathTeam.OrchestratorAPI.Activities
{
    [LocalizedDisplayName(nameof(Resources.OrchestratorScope_DisplayName))]
    [LocalizedDescription(nameof(Resources.OrchestratorScope_Description))]
    public class OrchestratorScope : ContinuableAsyncNativeActivity
    {
        #region Properties

        [Browsable(false)]
        public ActivityAction<IObjectContainer​> Body { get; set; }

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.Timeout_DisplayName))]
        [LocalizedDescription(nameof(Resources.Timeout_Description))]
        public InArgument<int> TimeoutMS { get; set; } = 60000;

        [LocalizedDisplayName(nameof(Resources.OrchestratorScope_OrchestratorName_DisplayName))]
        [LocalizedDescription(nameof(Resources.OrchestratorScope_OrchestratorName_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> OrchestratorName { get; set; }

        [LocalizedDisplayName(nameof(Resources.OrchestratorScope_ClientID_DisplayName))]
        [LocalizedDescription(nameof(Resources.OrchestratorScope_ClientID_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<SecureString> ClientID { get; set; }

        [LocalizedDisplayName(nameof(Resources.OrchestratorScope_UserKey_DisplayName))]
        [LocalizedDescription(nameof(Resources.OrchestratorScope_UserKey_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<SecureString> UserKey { get; set; }

        [LocalizedDisplayName(nameof(Resources.OrchestratorScope_TenantName_DisplayName))]
        [LocalizedDescription(nameof(Resources.OrchestratorScope_TenantName_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> TenantName { get; set; }

        [LocalizedDisplayName(nameof(Resources.OrchestratorScope_ExistingAccessToken_DisplayName))]
        [LocalizedDescription(nameof(Resources.OrchestratorScope_ExistingAccessToken_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> ExistingAccessToken { get; set; }

        [LocalizedDisplayName(nameof(Resources.OrchestratorScope_AccessToken_DisplayName))]
        [LocalizedDescription(nameof(Resources.OrchestratorScope_AccessToken_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<string> AccessToken { get; set; }

        // A tag used to identify the scope in the activity context
        internal static string ParentContainerPropertyTag => "ScopeActivity";

        // Object Container: Add strongly-typed objects here and they will be available in the scope's child activities.
        private readonly IObjectContainer _objectContainer;

        #endregion


        #region Constructors

        public OrchestratorScope(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;

            Body = new ActivityAction<IObjectContainer>
            {
                Argument = new DelegateInArgument<IObjectContainer> (ParentContainerPropertyTag),
                Handler = new Sequence { DisplayName = Resources.Do }
            };
        }

        public OrchestratorScope() : this(new ObjectContainer())
        {

        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (OrchestratorName == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(OrchestratorName)));
            if (ClientID == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ClientID)));
            if (UserKey == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(UserKey)));
            if (TenantName == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(TenantName)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<NativeActivityContext>> ExecuteAsync(NativeActivityContext  context, CancellationToken cancellationToken)
        {
            // Inputs
            var timeout = TimeoutMS.Get(context);
            var orchestratorName = OrchestratorName.Get(context);
            var clientID = ClientID.Get(context);
            var userKey = UserKey.Get(context);
            var tenantName = TenantName.Get(context);
            var existingAccessToken = ExistingAccessToken.Get(context);

            // Set a timeout on the execution
            var task = ExecuteWithTimeout(context, cancellationToken);
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) != task) throw new TimeoutException(Resources.Timeout_Error);
            await task;

            return (ctx) => {
                // Schedule child activities
                if (Body != null)
				    ctx.ScheduleAction<IObjectContainer>(Body, _objectContainer, OnCompleted, OnFaulted);

                // Outputs
                AccessToken.Set(ctx, null);
            };
        }

        private async Task ExecuteWithTimeout(NativeActivityContext context, CancellationToken cancellationToken = default)
        {
            ///////////////////////////
            // Add execution logic HERE
            ///////////////////////////
        }

        #endregion


        #region Events

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            faultContext.CancelChildren();
            Cleanup();
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            Cleanup();
        }

        #endregion


        #region Helpers
        
        private void Cleanup()
        {
            var disposableObjects = _objectContainer.Where(o => o is IDisposable);
            foreach (var obj in disposableObjects)
            {
                if (obj is IDisposable dispObject)
                    dispObject.Dispose();
            }
            _objectContainer.Clear();
        }

        #endregion
    }
}

