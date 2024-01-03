import os
import platform
import subprocess

def create_updated_shortcuts_v4(base_folder, subfolders):
    # Function to create a shortcut
    def create_shortcut(target, shortcut_path):
        if platform.system() == 'Windows':
            # Windows uses a VBScript to create a shortcut
            vbs_script = f'''
            Set oWS = WScript.CreateObject("WScript.Shell")
            sLinkFile = "{shortcut_path}"
            Set oLink = oWS.CreateShortcut(sLinkFile)
            oLink.TargetPath = "{target}"
            oLink.Save
            '''

            with open("create_shortcut.vbs", "w") as file:
                file.write(vbs_script)
            
            subprocess.call(['cscript.exe', 'create_shortcut.vbs'])
            os.remove("create_shortcut.vbs")
        else:
            # Unix-based systems use a symbolic link
            if not os.path.exists(shortcut_path) and not os.path.islink(shortcut_path):
                os.symlink(target, shortcut_path)

    # Create shortcuts in each subfolder
    for subfolder in subfolders:
        folder_path = os.path.join(base_folder, subfolder)
        if os.path.exists(folder_path):
            # Define the target and the shortcut path
            target = os.path.join(folder_path, "html", "index.html")
            shortcut_name = "index.html.link" if platform.system() != 'Windows' else f"{subfolder}.lnk"
            shortcut_path = os.path.join(folder_path, shortcut_name)

            create_shortcut(target, shortcut_path)

    return "Shortcuts created in all subfolders with updated path."

# Define the base folder and the subfolders list
base_folder = "D:\work\Client\_Documentation"
subfolders = [
    "_Build", "Aatrix", "Aatrix_CompanyDatabase", "AatrixData", "Aatrix_Database",
    "Aatrix_FileSystem", "Aatrix_Form", "AatrixForms", "Aatrix_Log", "Aatrix_System",
    "Aatrix.TaxTables", "Aatrix_Telemetry", "Aatrix_XML", "AufCache", "AufDatabase",
    "AuditFile", "AuditTool", "Builder", "BuildAUFDBHeader", "CBuilder", "CheckStatus",
    "Chilkat", "CompanySetup", "Crypto", "DecryptTool", "EFile", "EFW2Importer",
    "FormEditor", "GenerateTaxTableXML", "GenAUFxml", "GoAatrix", "HR", "Icon", "Importer",
    "Include", "MappingTool", "Packages", "PreReqCheckDll", "Registration", "Release",
    "ReportFile", "Signature", "SolutionItems", "SQLCipher", "SQLCipher.NET", "TBarCode10",
    "TaxTableCreator", "Updater", "Ultimate Grid", "VendorDll", "Viewer", "W2EasyEfiler",
    "W2eMailWizard"
]

# Call the function to create updated shortcuts with new path format
create_updated_shortcuts_v4(base_folder, subfolders)
