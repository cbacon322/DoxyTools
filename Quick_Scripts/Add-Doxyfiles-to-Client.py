import os

# Base directory containing the project subfolders
base_dir = r'D:\work\Client'  # Replace with the path to your projects folder

# Path to the Doxyfile template
doxyfile_template_path = r'D:\work\Client\Documentation_Tools\Doxyfile'  # Replace with the path to your Doxyfile template

# List of project names
project_names = [
    "_Build",
    #"_Output",
    #"_Output_d",
    "Aatrix",
    "Aatrix_CompanyDatabase",
    "AatrixData",
    "Aatrix_Database",
    "Aatrix_FileSystem",
    "Aatrix_Form",
    "AatrixForms",
    "Aatrix_Log",
    "Aatrix_System",
    "Aatrix.TaxTables",
    "Aatrix_Telemetry",
    "Aatrix_XML",
    "AufCache",
    "AufDatabase",
    "AuditFile",
    "AuditTool",
    "Builder",
    "BuildAUFDBHeader",
    "CBuilder",
    "CheckStatus",
    "Chilkat",
    "CompanySetup",
    "Crypto",
    "DecryptTool",
    "EFile",
    "EFW2Importer",
    "FormEditor",
    "GenerateTaxTableXML",
    "GenAUFxml",
    "GoAatrix",
    "HR",
    "Icon",
    "Importer",
    "Include",
    "MappingTool",
    "Packages",
    "PreReqCheckDll",
    "Registration",
    "Release",
    "ReportFile",
    "Signature",
    "SolutionItems",
    "SQLCipher",
    "SQLCipher.NET",
    "TBarCode10",
    "TaxTableCreator",
    "Updater",
    "Ultimate Grid",
    "VendorDll",
    "Viewer",
    "W2EasyEfiler",
    "W2eMailWizard"
]

def create_doxyfile_for_project(project_name):
    # Path to the project folder
    project_folder = os.path.join(base_dir, project_name)

    # Check if the project folder exists
    if not os.path.isdir(project_folder):
        print(f"Project folder not found: {project_folder}")
        return

    # Path to the new Doxyfile in the project folder
    doxyfile_path = os.path.join(project_folder, 'Doxyfile')

    # Read the template Doxyfile
    with open(doxyfile_template_path, 'r') as file:
        template_content = file.read()

    # Replace the placeholder with the actual project name
    doxyfile_content = template_content.replace('[PROJECT_NAME]', project_name)

    # Write the new Doxyfile
    with open(doxyfile_path, 'w') as file:
        file.write(doxyfile_content)
    print(f"Doxyfile created for project: {project_name}")

# Create a Doxyfile for each project
for project_name in project_names:
    create_doxyfile_for_project(project_name)

print("Doxyfiles creation complete.")
