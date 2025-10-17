2025-10-17 20:14:53.062 | info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
2025-10-17 20:14:53.062 |       Request starting HTTP/1.1 OPTIONS http://localhost:8080/api/UserPreferences - - -
2025-10-17 20:14:53.062 | info: Microsoft.AspNetCore.Cors.Infrastructure.CorsService[4]
2025-10-17 20:14:53.062 |       CORS policy execution successful.
2025-10-17 20:14:53.062 | info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
2025-10-17 20:14:53.062 |       Request finished HTTP/1.1 OPTIONS http://localhost:8080/api/UserPreferences - 204 - - 0.2800ms
2025-10-17 20:14:53.064 | info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
2025-10-17 20:14:53.064 |       Request starting HTTP/1.1 PUT http://localhost:8080/api/UserPreferences - application/json 122
2025-10-17 20:14:53.064 | info: Microsoft.AspNetCore.Cors.Infrastructure.CorsService[4]
2025-10-17 20:14:53.064 |       CORS policy execution successful.
2025-10-17 20:14:53.066 | info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
2025-10-17 20:14:53.066 |       Executing endpoint 'backend.Web.Controllers.UserPreferencesController.SaveUserPreferences (backend.Web)'
2025-10-17 20:14:53.066 | info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[102]
2025-10-17 20:14:53.066 |       Route matched with {action = "SaveUserPreferences", controller = "UserPreferences"}. Executing controller action with signature System.Threading.Tasks.Task`1[Microsoft.AspNetCore.Mvc.ActionResult`1[backend.Application.Common.Models.Result]] SaveUserPreferences(backend.Application.UserPreferences.Commands.SaveUserPreferences.SaveUserPreferencesCommand) on controller backend.Web.Controllers.UserPreferencesController (backend.Web).
2025-10-17 20:14:53.077 | fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
2025-10-17 20:14:53.077 |       Failed executing DbCommand (1ms) [Parameters=[@p0='?' (DbType = Guid), @p1='?' (DbType = DateTime), @p2='?' (Size = 4000), @p3='?' (DbType = Boolean), @p4='?' (DbType = Guid), @p5='?' (DbType = Boolean), @p6='?' (DbType = Int32), @p7='?' (DbType = DateTime), @p8='?' (Size = 4000), @p9='?' (DbType = Boolean), @p10='?' (DbType = Int32), @p12='?' (DbType = Guid), @p11='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
2025-10-17 20:14:53.077 |       INSERT INTO `UserPreferences` (`UserProfileId`, `Created`, `CreatedBy`, `EmailNotificationsEnabled`, `Id`, `InAppNotificationsEnabled`, `Language`, `LastModified`, `LastModifiedBy`, `SmsNotificationsEnabled`, `Theme`)
2025-10-17 20:14:53.077 |       VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10);
2025-10-17 20:14:53.077 |       UPDATE `UserProfiles` SET `UserPreferenceUserProfileId` = @p11
2025-10-17 20:14:53.077 |       WHERE `Id` = @p12;
2025-10-17 20:14:53.077 |       SELECT ROW_COUNT();
2025-10-17 20:14:53.081 | fail: Microsoft.EntityFrameworkCore.Update[10000]
2025-10-17 20:14:53.081 |       An exception occurred in the database while saving changes for context type 'backend.Infrastructure.Data.ApplicationDbContext'.
2025-10-17 20:14:53.081 |       Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
2025-10-17 20:14:53.081 |        ---> MySqlConnector.MySqlException (0x80004005): Duplicate entry '9ff7af78-7e6d-4ee3-801a-268328542027' for key 'UserPreferences.PRIMARY'
2025-10-17 20:14:53.081 |          at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 936
2025-10-17 20:14:53.081 |          at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
2025-10-17 20:14:53.081 |          at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
2025-10-17 20:14:53.081 |          at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
2025-10-17 20:14:53.081 |          at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
2025-10-17 20:14:53.081 |          at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
2025-10-17 20:14:53.081 |          at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 350
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          --- End of inner exception stack trace ---
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Pomelo.EntityFrameworkCore.MySql.Storage.Internal.MySqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |       Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
2025-10-17 20:14:53.081 |        ---> MySqlConnector.MySqlException (0x80004005): Duplicate entry '9ff7af78-7e6d-4ee3-801a-268328542027' for key 'UserPreferences.PRIMARY'
2025-10-17 20:14:53.081 |          at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 936
2025-10-17 20:14:53.081 |          at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
2025-10-17 20:14:53.081 |          at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
2025-10-17 20:14:53.081 |          at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
2025-10-17 20:14:53.081 |          at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
2025-10-17 20:14:53.081 |          at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
2025-10-17 20:14:53.081 |          at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 350
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          --- End of inner exception stack trace ---
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Pomelo.EntityFrameworkCore.MySql.Storage.Internal.MySqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
2025-10-17 20:14:53.081 |          at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 | fail: backend.Application.UserPreferences.Commands.SaveUserPreferences.SaveUserPreferencesCommand[0]
2025-10-17 20:14:53.083 |       backend Request: Unhandled Exception for Request SaveUserPreferencesCommand backend.Application.UserPreferences.Commands.SaveUserPreferences.SaveUserPreferencesCommand
2025-10-17 20:14:53.083 |       Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
2025-10-17 20:14:53.083 |        ---> MySqlConnector.MySqlException (0x80004005): Duplicate entry '9ff7af78-7e6d-4ee3-801a-268328542027' for key 'UserPreferences.PRIMARY'
2025-10-17 20:14:53.083 |          at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 936
2025-10-17 20:14:53.083 |          at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
2025-10-17 20:14:53.083 |          at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
2025-10-17 20:14:53.083 |          at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
2025-10-17 20:14:53.083 |          at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
2025-10-17 20:14:53.083 |          at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
2025-10-17 20:14:53.083 |          at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 350
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          --- End of inner exception stack trace ---
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Pomelo.EntityFrameworkCore.MySql.Storage.Internal.MySqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.083 |          at backend.Application.UserPreferences.Commands.SaveUserPreferences.SaveUserPreferencesCommandHandler.Handle(SaveUserPreferencesCommand request, CancellationToken cancellationToken) in /src/src/Application/UserPreferences/Commands/SaveUserPreferences/SaveUserPreferencesCommandHandler.cs:line 55
2025-10-17 20:14:53.083 |          at backend.Application.Common.Behaviours.PerformanceBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/PerformanceBehaviour.cs:line 23
2025-10-17 20:14:53.083 |          at backend.Application.Common.Behaviours.ValidationBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/ValidationBehaviour.cs:line 32
2025-10-17 20:14:53.083 |          at backend.Application.Common.Behaviours.AuthorizationBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/AuthorizationBehaviour.cs:line 46
2025-10-17 20:14:53.083 |          at backend.Application.Common.Behaviours.UnhandledExceptionBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/UnhandledExceptionBehaviour.cs:line 19
2025-10-17 20:14:53.083 | info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[105]
2025-10-17 20:14:53.083 |       Executed action backend.Web.Controllers.UserPreferencesController.SaveUserPreferences (backend.Web) in 17.3699ms
2025-10-17 20:14:53.083 | info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
2025-10-17 20:14:53.083 |       Executed endpoint 'backend.Web.Controllers.UserPreferencesController.SaveUserPreferences (backend.Web)'
2025-10-17 20:14:53.085 | fail: Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware[1]
2025-10-17 20:14:53.085 |       An unhandled exception has occurred while executing the request.
2025-10-17 20:14:53.085 |       Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
2025-10-17 20:14:53.085 |        ---> MySqlConnector.MySqlException (0x80004005): Duplicate entry '9ff7af78-7e6d-4ee3-801a-268328542027' for key 'UserPreferences.PRIMARY'
2025-10-17 20:14:53.085 |          at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 936
2025-10-17 20:14:53.085 |          at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
2025-10-17 20:14:53.085 |          at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
2025-10-17 20:14:53.085 |          at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
2025-10-17 20:14:53.085 |          at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
2025-10-17 20:14:53.085 |          at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
2025-10-17 20:14:53.085 |          at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 350
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          --- End of inner exception stack trace ---
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Pomelo.EntityFrameworkCore.MySql.Storage.Internal.MySqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.085 |          at backend.Application.UserPreferences.Commands.SaveUserPreferences.SaveUserPreferencesCommandHandler.Handle(SaveUserPreferencesCommand request, CancellationToken cancellationToken) in /src/src/Application/UserPreferences/Commands/SaveUserPreferences/SaveUserPreferencesCommandHandler.cs:line 55
2025-10-17 20:14:53.085 |          at backend.Application.Common.Behaviours.PerformanceBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/PerformanceBehaviour.cs:line 23
2025-10-17 20:14:53.085 |          at backend.Application.Common.Behaviours.ValidationBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/ValidationBehaviour.cs:line 32
2025-10-17 20:14:53.085 |          at backend.Application.Common.Behaviours.AuthorizationBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/AuthorizationBehaviour.cs:line 46
2025-10-17 20:14:53.085 |          at backend.Application.Common.Behaviours.UnhandledExceptionBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/UnhandledExceptionBehaviour.cs:line 19
2025-10-17 20:14:53.085 |          at backend.Web.Controllers.UserPreferencesController.SaveUserPreferences(SaveUserPreferencesCommand command) in /src/src/Web/Controllers/UserPreferencesController.cs:line 45
2025-10-17 20:14:53.085 |          at lambda_method739(Closure, Object)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|7_0(Endpoint endpoint, Task requestTask, ILogger logger)
2025-10-17 20:14:53.085 |          at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.<Invoke>g__Awaited|10_0(ExceptionHandlerMiddlewareImpl middleware, HttpContext context, Task task)
2025-10-17 20:14:53.087 | fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
2025-10-17 20:14:53.087 |       An unhandled exception has occurred while executing the request.
2025-10-17 20:14:53.087 |       System.InvalidOperationException: The exception handler configured on ExceptionHandlerOptions produced a 404 status response. This InvalidOperationException containing the original exception was thrown since this is often due to a misconfigured ExceptionHandlingPath. If the exception handler is expected to return 404 status responses then set AllowStatusCode404Response to true.
2025-10-17 20:14:53.087 |        ---> Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
2025-10-17 20:14:53.087 |        ---> MySqlConnector.MySqlException (0x80004005): Duplicate entry '9ff7af78-7e6d-4ee3-801a-268328542027' for key 'UserPreferences.PRIMARY'
2025-10-17 20:14:53.087 |          at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 936
2025-10-17 20:14:53.087 |          at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
2025-10-17 20:14:53.087 |          at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
2025-10-17 20:14:53.087 |          at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
2025-10-17 20:14:53.087 |          at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
2025-10-17 20:14:53.087 |          at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
2025-10-17 20:14:53.087 |          at MySqlConnector.MySqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 350
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          --- End of inner exception stack trace ---
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Pomelo.EntityFrameworkCore.MySql.Storage.Internal.MySqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
2025-10-17 20:14:53.087 |          at backend.Application.UserPreferences.Commands.SaveUserPreferences.SaveUserPreferencesCommandHandler.Handle(SaveUserPreferencesCommand request, CancellationToken cancellationToken) in /src/src/Application/UserPreferences/Commands/SaveUserPreferences/SaveUserPreferencesCommandHandler.cs:line 55
2025-10-17 20:14:53.087 |          at backend.Application.Common.Behaviours.PerformanceBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/PerformanceBehaviour.cs:line 23
2025-10-17 20:14:53.087 |          at backend.Application.Common.Behaviours.ValidationBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/ValidationBehaviour.cs:line 32
2025-10-17 20:14:53.087 |          at backend.Application.Common.Behaviours.AuthorizationBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/AuthorizationBehaviour.cs:line 46
2025-10-17 20:14:53.087 |          at backend.Application.Common.Behaviours.UnhandledExceptionBehaviour`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /src/src/Application/Common/Behaviours/UnhandledExceptionBehaviour.cs:line 19
2025-10-17 20:14:53.087 |          at backend.Web.Controllers.UserPreferencesController.SaveUserPreferences(SaveUserPreferencesCommand command) in /src/src/Web/Controllers/UserPreferencesController.cs:line 45
2025-10-17 20:14:53.087 |          at lambda_method739(Closure, Object)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|7_0(Endpoint endpoint, Task requestTask, ILogger logger)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.<Invoke>g__Awaited|10_0(ExceptionHandlerMiddlewareImpl middleware, HttpContext context, Task task)
2025-10-17 20:14:53.087 |          --- End of inner exception stack trace ---
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.HandleException(HttpContext context, ExceptionDispatchInfo edi)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.<Invoke>g__Awaited|10_0(ExceptionHandlerMiddlewareImpl middleware, HttpContext context, Task task)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
2025-10-17 20:14:53.087 |          at NSwag.AspNetCore.Middlewares.SwaggerUiIndexMiddleware.Invoke(HttpContext context)
2025-10-17 20:14:53.087 |          at NSwag.AspNetCore.Middlewares.RedirectToIndexMiddleware.Invoke(HttpContext context)
2025-10-17 20:14:53.087 |          at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)
2025-10-17 20:14:53.089 | info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
2025-10-17 20:14:53.089 |       Request finished HTTP/1.1 PUT http://localhost:8080/api/UserPreferences - 500 - text/plain;+charset=utf-8 24.1843ms
2025-10-17 20:14:53.089 | info: Microsoft.AspNetCore.Hosting.Diagnostics[16]
2025-10-17 20:14:53.089 |       Request reached the end of the middleware pipeline without being handled by application code. Request path: PUT http://localhost:8080/api/UserPreferences, Response status code: 500