﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CloverLeaf.Common.Infrastructure.Services.Interfaces
{
    public interface IShell
    {
        WindowState WindowState { get; set; }
        bool Activate();
        void DragMove();
        void Close();
    }
}
