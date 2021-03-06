﻿using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using MvvmTools.Core.Services;
using Ninject;

namespace MvvmTools
{
    /// <summary>
    /// The base implementation of a command.
    /// </summary>
    internal abstract class BaseCommand : OleMenuCommand
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommand" /> class.
        /// </summary>
        /// <param name="id">The id for the command.</param>
        protected BaseCommand(CommandID id)
            : base(BaseCommand_Execute, id)
        {
            BeforeQueryStatus += BaseCommand_BeforeQueryStatus;
        }

        #endregion Constructors

        #region Properties

        [Inject]
        public IMvvmToolsPackage Package { get; set; }

        [Inject]
        public ISettingsService SettingsService { get; set; }

        [Inject]
        public IKernel Kernel { get; set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        protected IServiceProvider ServiceProvider => Package;

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the BeforeQueryStatus event of the BaseCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void BaseCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            var command = sender as BaseCommand;
            command?.OnBeforeQueryStatus();
        }

        /// <summary>
        /// Handles the Execute event of the BaseCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void BaseCommand_Execute(object sender, EventArgs e)
        {
            var command = sender as BaseCommand;
            command?.OnExecute();
        }

        #endregion Event Handlers

        #region Virtual Methods

        /// <summary>
        /// Called to update the current status of the command.
        /// </summary>
        protected virtual void OnBeforeQueryStatus()
        {
            // By default, commands are always enabled.
            Enabled = true;
        }

        /// <summary>
        /// Called to execute the command.
        /// </summary>
        protected virtual void OnExecute()
        {
            Trace.WriteLine($"{GetType().Name}.OnExecute() invoked");
        }

        #endregion Virtual Methods
    }
}
