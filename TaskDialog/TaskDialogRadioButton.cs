﻿using System;

using TaskDialogFlags = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_FLAGS;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TaskDialogRadioButton : TaskDialogControl
    {
        private string _text;

        private int _radioButtonID;

        private bool _enabled = true;

        private bool _checked;

        private TaskDialogRadioButtonCollection _collection;

        private bool _ignoreRadioButtonClickedNotification;

        /// <summary>
        /// Occurs when the value of the <see cref="Checked"/> property has changed
        /// while this control is bound to a task dialog.
        /// </summary>
        public event EventHandler CheckedChanged;

        /// <summary>
        /// 
        /// </summary>
        public TaskDialogRadioButton()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public TaskDialogRadioButton(string text)
            : this()
        {
            _text = text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This property can be set while the dialog is shown.
        /// </remarks>
        public bool Enabled
        {
            get => _enabled;

            set
            {
                DenyIfBoundAndNotCreated();

                // Check if we can update the button.
                if (CanUpdate())
                {
                    BoundPage?.BoundTaskDialog.SetRadioButtonEnabled(
                            _radioButtonID,
                            value);
                }

                _enabled = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get => _text;

            set
            {
                DenyIfBound();

                _text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This property can be set to <c>true</c> while the dialog is shown (except
        /// from within the <see cref="CheckedChanged"/> event).
        /// </remarks>
        public bool Checked
        {
            get => _checked;

            set
            {
                DenyIfBoundAndNotCreated();

                // Unchecking a radio button is not possible in the task dialog.
                // TODO: Should we throw only if the new value is different than the
                // old one?
                if (BoundPage != null && !value)
                    throw new InvalidOperationException(
                            "Cannot uncheck a radio button while it is bound to a task dialog.");

                if (BoundPage == null)
                {
                    _checked = value;

                    // If we are part of a collection, set the checked value of
                    // all other buttons to False.
                    // Note that this does not handle buttons that are added later to
                    // the collection.
                    if (_collection != null && value)
                    {
                        foreach (TaskDialogRadioButton radioButton in _collection)
                            radioButton._checked = radioButton == this;
                    }
                }
                else
                {
                    // Don't allow to click the radio button if we are currently in the
                    // RadioButtonClicked notification handler - see comments in 
                    // HandleRadioButtonClicked().
                    if (BoundPage.BoundTaskDialog.RadioButtonClickedStackCount > 0)
                        throw new InvalidOperationException(
                                $"Cannot set the " +
                                $"{nameof(TaskDialogRadioButton)}.{nameof(Checked)} " +
                                $"property from within the " +
                                $"{nameof(TaskDialogRadioButton)}.{nameof(CheckedChanged)} " +
                                $"event of one of the radio buttons of the current task dialog.");

                    // Click the radio button which will (recursively) raise the
                    // RadioButtonClicked notification. However, we ignore the
                    // notification and then raise the events afterwards (because
                    // the task dialog actually selects the radio button only
                    // after the notification handler returned - see comments in
                    // HandleRadioButtonClicked().
                    _ignoreRadioButtonClickedNotification = true;
                    try
                    {
                        BoundPage.BoundTaskDialog.ClickRadioButton(
                                _radioButtonID);
                    }
                    finally
                    {
                        _ignoreRadioButtonClickedNotification = false;
                    }

                    // Now raise the events.
                    HandleRadioButtonClicked();
                }
            }
        }

        internal int RadioButtonID
        {
            get => _radioButtonID;
        }

        internal TaskDialogRadioButtonCollection Collection
        {
            get => _collection;
            set => _collection = value;
        }

        internal override bool IsCreatable
        {
            get => base.IsCreatable && !TaskDialogPage.IsNativeStringNullOrEmpty(_text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _text ?? base.ToString();
        }

        internal TaskDialogFlags Bind(TaskDialogPage page, int radioButtonID)
        {
            TaskDialogFlags result = Bind(page);
            _radioButtonID = radioButtonID;

            return result;
        }

        internal void HandleRadioButtonClicked()
        {
            // Check if we need to ignore the notification when it is caused by sending
            // the ClickRadioButton message.
            if (_ignoreRadioButtonClickedNotification)
                return;

            //// Note: We do not allow to set the "Checked" property of any radio button
            //// of the current task dialog while we are within the RadioButtonClicked
            //// notification handler. This is because the logic of the task dialog is
            //// such that the radio button will be selected AFTER the callback returns
            //// (not before it is called), at least when the event is caused by code
            //// sending the ClickRadioButton message. This is mentioned in the
            //// documentation for TDM_CLICK_RADIO_BUTTON:
            //// "The specified radio button ID is sent to the TaskDialogCallbackProc
            //// callback function as part of a TDN_RADIO_BUTTON_CLICKED notification code.
            //// After the callback function returns, the radio button will be selected."
            //// 
            //// While we handle this by ignoring the RadioButtonClicked notification
            //// when it is caused by a ClickRadioButton message, and then raise the
            //// events after the notification handler returned, this still seems to
            //// cause problems for RadioButtonClicked notifications initially caused
            //// by the user clicking the radio button in the UI.
            //// 
            //// For example, consider a scenario with two radio buttons [ID 1 and 2],
            //// and the user added an event handler to automatically select the first
            //// radio button (ID 1) when the second one (ID 2) is selected in the UI.
            //// This means the stack will then look as follows:
            //// Show() ->
            //// Callback: RadioButtonClicked [ID 2] ->
            //// SendMessage: ClickRadioButton [ID 1] ->
            //// Callback: RadioButtonClicked [ID 1]
            ////
            //// However, when the initial RadioButtonClicked handler (ID 2) returns, the
            //// TaskDialog again calls the handler for ID 1 (which wouldn't be a problem),
            //// and then again calls it for ID 2, which is unexpected (and it doesn't
            //// seem that we can prevent this by returning S_FALSE in the notification
            //// handler). Additionally, after that it even seems we get an endless loop
            //// of RadioButtonClicked notifications even when we don't send any further
            //// messages to the dialog.

            if (!_checked)
            {
                // Need to copy the dialog reference because it can become null if the
                // dialog is navigated from the event handler.
                TaskDialog boundDialog = BoundPage.BoundTaskDialog;
                checked
                {
                    boundDialog.RadioButtonClickedStackCount++;
                }
                try
                {
                    _checked = true;

                    // Before raising the CheckedChanged event for the current button,
                    // uncheck the other radio buttons and call their events.
                    foreach (TaskDialogRadioButton radioButton in BoundPage.RadioButtons)
                    {
                        if (radioButton != this && radioButton._checked)
                        {
                            radioButton._checked = false;
                            radioButton.OnCheckedChanged(EventArgs.Empty);
                        }
                    }

                    // Finally, call the event for the current button.
                    OnCheckedChanged(EventArgs.Empty);
                }
                finally
                {
                    boundDialog.RadioButtonClickedStackCount--;
                }
            }
        }

        private protected override void UnbindCore()
        {
            _radioButtonID = 0;

            base.UnbindCore();
        }

        private protected override void ApplyInitializationCore()
        {
            // Re-set the properties so they will make the necessary calls.
            if (!_enabled)
                Enabled = _enabled;
        }

        private bool CanUpdate()
        {
            // Only update the button when bound to a task dialog and we are not
            // waiting for the Navigated event. In the latter case we don't throw
            // an exception however, because ApplyInitialization will be called in
            // the Navigated handler that does the necessary updates.
            return BoundPage?.BoundTaskDialog
                    .WaitingForNavigatedEvent == false;
        }

        private void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }
    }
}
