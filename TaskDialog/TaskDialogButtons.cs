﻿using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum TaskDialogButtons : int
    {
        /// <summary>
        /// 
        /// </summary>
        OK = 1 << 0,

        /// <summary>
        /// 
        /// </summary>
        Yes = 1 << 1,

        /// <summary>
        /// 
        /// </summary>
        No = 1 << 2,

        /// <summary>
        /// 
        /// </summary>
        Cancel = 1 << 3,

        /// <summary>
        /// 
        /// </summary>
        Retry = 1 << 4,

        /// <summary>
        /// 
        /// </summary>
        Close = 1 << 5,

        /// <summary>
        /// 
        /// </summary>
        Abort = 1 << 16,

        /// <summary>
        /// 
        /// </summary>
        Ignore = 1 << 17,

        /// <summary>
        /// 
        /// </summary>
        TryAgain = 1 << 18,

        /// <summary>
        /// 
        /// </summary>
        Continue = 1 << 19,

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note: Clicking this button will not close the dialog, but will raise the
        /// <see cref="TaskDialog.Help"/> event.
        /// </remarks>
        Help = 1 << 20
    }
}
