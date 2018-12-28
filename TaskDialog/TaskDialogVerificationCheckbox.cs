﻿using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TaskDialogVerificationCheckbox : TaskDialogControl
    {
        private string text;

        private bool @checked;


        /// <summary>
        /// 
        /// </summary>
        public event EventHandler CheckedChanged;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogVerificationCheckbox()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public TaskDialogVerificationCheckbox(string text)
            : base()
        {
            this.text = text;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get => this.text;

            set {
                this.text = value;

                this.boundTaskDialogContents?.DenyIfBound();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Checked
        {
            get => this.@checked;

            set
            {
                if (this.boundTaskDialogContents == null)
                {
                    this.@checked = value;
                }
                else
                {
                    // Click the checkbox which should then raise the event, where we
                    // will update the checked state.
                    this.boundTaskDialogContents.BoundTaskDialog.ClickVerification(
                            value);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Focus()
        {
            this.DenyIfNotBound();
            this.boundTaskDialogContents.BoundTaskDialog.ClickVerification(
                    this.@checked,
                    true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.text ?? base.ToString();
        }


        internal void HandleCheckboxClicked(bool @checked)
        {
            // Only raise the event if the state actually changed.
            if (@checked != this.@checked)
            {
                this.@checked = @checked;
                this.OnCheckedChanged(EventArgs.Empty);
            }
        }

        internal override TaskDialogFlags GetFlags()
        {
            var flags = base.GetFlags();

            if (this.@checked)
                flags |= TaskDialogFlags.VerificationFlagChecked;

            return flags;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnCheckedChanged(EventArgs e)
        {
            this.CheckedChanged?.Invoke(this, e);
        }
    }
}
