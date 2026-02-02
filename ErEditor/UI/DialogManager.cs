using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI
{
    public static class DialogManager
    {
        // method shouldn't be called several times to open new window. Save window in DialogManager as a field and redirect
        // requests to Open it by focusing it?
        public static ErSchema? CreateNewErSchemaWindow()
        {
            NewErSchemaWindow newErSchemaWindow = new();
            switch (newErSchemaWindow.ShowDialog())
            {
                case DialogResult.OK:
                    {
                        ErSchema newSchema = ErSchemaFileManager.NewErSchema(
                            newErSchemaWindow.ErSchemaName,
                            newErSchemaWindow.ErSchemaFileName,
                            newErSchemaWindow.ErSchemaFolderPath
                        );
                        newErSchemaWindow.Dispose();
                        return newSchema;
                    }

                default:
                    {
                        newErSchemaWindow.Dispose();
                        return null;
                    }
            }
        }
        public static ErSchema? OpenSchemaWindow()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult dr = ofd.ShowDialog();
            switch (dr)
            {
                case DialogResult.OK:
                    var schema = ErSchemaFileManager.OpenErSchema(ofd.FileName);
                    return schema;

                default:
                    return null;
            }
            
        }
    }
}
