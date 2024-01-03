import os

# Define the folder where subfolders will be created
parent_folder = "D:\work\Client\_Documentation"

# List of subfolders to create
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

# Creating subfolders inside the _Documentation folder
for subfolder in subfolders:
    # Construct the full path for the new subfolder
    path = os.path.join(parent_folder, subfolder)

    # Create the subfolder if it doesn't exist
    if not os.path.exists(path):
        os.makedirs(path)