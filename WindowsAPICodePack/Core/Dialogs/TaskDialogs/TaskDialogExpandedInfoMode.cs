//Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAPICodePack
{
    /// <summary>
    /// Specifies the options for expand/collapse sections in dialogs.
    /// </summary>
    public enum TaskDialogExpandedDetailsLocation
    {
        /// <summary>
        /// Do not show the content.
        /// </summary>
        Hide, 
        /// <summary>
        /// Show the content.
        /// </summary>
        ExpandContent,
        /// <summary>
        /// Expand the footer content.
        /// </summary>
        ExpandFooter
    }
}