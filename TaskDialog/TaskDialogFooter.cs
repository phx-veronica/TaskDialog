﻿using System;
using System.ComponentModel;

using TaskDialogFlags = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_FLAGS;
using TaskDialogTextElement = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_ELEMENTS;
using TaskDialogIconElement = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_ICON_ELEMENTS;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class TaskDialogFooter : TaskDialogControl
    {
        private string text;

        private TaskDialogIcon icon;

        private IntPtr iconHandle;

        private bool boundFooterIconIsFromHandle;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogFooter()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public TaskDialogFooter(string text)
            : this()
        {
            this.text = text;
        }


        /// <summary>
        /// Gets or sets the text to be displayed in the dialog's footer area.
        /// </summary>
        /// <remarks>
        /// This property can be set while the dialog is shown.
        /// </remarks>
        public string Text
        {
            get => this.text;

            set {
                this.DenyIfBoundAndNotCreated();

                // Update the text if we are bound.
                this.BoundTaskDialogContents?.BoundTaskDialog.UpdateTextElement(
                        TaskDialogTextElement.TDE_FOOTER,
                        value);

                this.text = value;
            }
        }

        /// <summary>
        /// Gets or sets the footer icon, if <see cref="IconHandle"/> is
        /// <see cref="IntPtr.Zero"/>.
        /// </summary>
        /// <remarks>
        /// This property can be set while the dialog is shown.
        /// </remarks>
        [DefaultValue(TaskDialogIcon.None)]
        public TaskDialogIcon Icon
        {
            get => this.icon;

            set
            {
                // See comments in property "TaskDialogContents.Icon".
                if (value < ushort.MinValue || (int)value > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this.DenyIfBoundAndNotCreated();

                if (this.BoundTaskDialogContents != null &&
                        this.boundFooterIconIsFromHandle)
                    throw new InvalidOperationException();

                this.BoundTaskDialogContents?.BoundTaskDialog.UpdateIconElement(
                        TaskDialogIconElement.TDIE_ICON_FOOTER,
                        (IntPtr)value);

                this.icon = value;
            }
        }

        /// <summary>
        /// Gets or sets the handle to the footer icon. When this member is not
        /// <see cref="IntPtr.Zero"/>, the <see cref="Icon"/> property will
        /// be ignored.
        /// </summary>
        /// <remarks>
        /// This property can be set while the dialog is shown.
        /// </remarks>
        [Browsable(false)]
        public IntPtr IconHandle
        {
            get => this.iconHandle;

            set
            {
                this.DenyIfBoundAndNotCreated();

                if (this.BoundTaskDialogContents != null &&
                        !this.boundFooterIconIsFromHandle)
                    throw new InvalidOperationException();

                this.BoundTaskDialogContents?.BoundTaskDialog.UpdateIconElement(
                        TaskDialogIconElement.TDIE_ICON_FOOTER,
                        value);

                this.iconHandle = value;
            }
        }


        internal override bool IsCreatable
        {
            get => base.IsCreatable && !TaskDialogContents.IsNativeStringNullOrEmpty(this.text);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.text ?? base.ToString();
        }


        internal TaskDialogFlags Bind(TaskDialogContents contents, out IntPtr footerIconValue)
        {
            var result = base.Bind(contents);

            footerIconValue = this.boundFooterIconIsFromHandle ? this.iconHandle : (IntPtr)this.icon;

            return result;
        }


        private protected override TaskDialogFlags GetFlagsCore()
        {
            var flags = base.GetFlagsCore();

            this.boundFooterIconIsFromHandle = this.iconHandle != IntPtr.Zero;
            if (this.boundFooterIconIsFromHandle)
                flags |= TaskDialogFlags.TDF_USE_HICON_FOOTER;

            return flags;
        }
    }
}
