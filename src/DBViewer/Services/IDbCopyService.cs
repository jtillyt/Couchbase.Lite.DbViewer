using System;
using System.Collections.Generic;
using System.Text;

namespace DBViewer.Services
{
    public interface IDbCopyService
    {
        bool CopyDbToLocalPath(string localDirectory, string remoteDirectory);
    }
}
