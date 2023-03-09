using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Mono.Cecil;
using System.Runtime.InteropServices.ComTypes;
using System.IO.Compression;
using System.Threading;
using Mono.Cecil.Rocks;

namespace DLLFileEncryption
{
    internal class Program
    {

        static List<string> methods = new List<string>() { "Start" , "Update" , "FixedUpdate" , "LateUpdate" , "OnGUI" , "OnDisable" , "OnEnable" };
        static List<string> fieldsNameSpace = new List<string>() { typeof(int).FullName, typeof(uint).FullName , typeof(ulong).FullName, typeof(long).FullName, typeof(short).FullName , typeof(ushort).FullName , typeof(bool).FullName , typeof(byte).FullName, typeof(double).FullName , typeof(float).FullName };
        [STAThread]
        static void Main(string[] args)
        {
            
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select A File";
            openDialog.Filter = "All Files (*.*)|*.*";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                string file = openDialog.FileName;
                if (file.EndsWith(".apk"))
                {
                    string managedPath = "assets/bin/Data/Managed/";
                    string dir = Path.GetDirectoryName(file) + "/Libary";
                    Directory.CreateDirectory(dir);
                    Console.WriteLine(dir);
                    ZipArchive zipArchive = new ZipArchive(new FileStream(file, FileMode.Open) , ZipArchiveMode.Update);
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    {
                        if (entry.FullName.Contains(managedPath))
                        {
                            Console.WriteLine(entry.Name);
                            using (FileStream fileStream = File.Open(dir + "/" + entry.Name, FileMode.OpenOrCreate))
                            {
                                using (Stream copy = entry.Open())
                                {
                                    copy.CopyTo(fileStream);
                                    copy.Close();
                                }
                                fileStream.Close();
                            }
                        }
                    }
                    Thread.Sleep(1000);
                    AssemblyDefinition assembly2 = ReadDLLFile(dir + "/Assembly-CSharp.dll");
                    EditDLL(assembly2);
                    WriteDLL(assembly2, dir + "/Assembly-CSharp.dll");
                    ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(managedPath + "Assembly-CSharp.dll");
                    //zipArchiveEntry.Delete();
                    
                    byte[] buffer = File.ReadAllBytes(dir + "/Assembly-CSharp.dll");
                    var st = zipArchiveEntry.Open();
                    st.Write(buffer, 0, buffer.Length);
                    st.Close();
                    zipArchive.Dispose();
                    return;
                }

                AssemblyDefinition assembly = ReadDLLFile(file);
                EditDLL(assembly);
                WriteDLL(assembly, file);
                 
            }
        }

        static string GetRandomString()
        {
            return Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "") + Guid.NewGuid().ToString().Replace("-", "");
        }

        static void EditDLL(AssemblyDefinition assembly)
        {
            
            ModuleDefinition module = assembly.MainModule;
            for (int i = 0; i < module.Types.Count; i++)
            {
                TypeDefinition type = module.Types[i];
                Console.WriteLine(type.Namespace + "." + type.Name);
                
                for (int k = 0; k < type.Fields.Count; k++)
                {
                    FieldDefinition propertyDefinition = type.Fields[k];
                    if (propertyDefinition.CustomAttributes.Count > 0 || propertyDefinition.HasCustomAttributes)
                    {
                        continue;
                    } 
                    else if(fieldsNameSpace.Contains(propertyDefinition.FieldType.FullName))
                    {
                        propertyDefinition.Name = GetRandomString();
                    }
                }
                if (type.BaseType != null)
                {
                    if (type.BaseType.Name != "MonoBehaviour")
                    {
                        type.Name = GetRandomString();
                        type.Namespace = GetRandomString();
                    }
                    
                }
                else
                {
                    type.Name = GetRandomString();
                    type.Namespace = GetRandomString();
                }
                bool hasJson = false;
                foreach(var m in type.Methods)
                {
                    if (m.Name == "Create" || m.Name == "create" || m.HasCustomAttributes)
                    {
                        hasJson = true;
                        
                    }                              
                }
                if(hasJson)
                {
                    continue;
                }
                
                foreach (var property in type.Properties)
                {
                    property.Name = GetRandomString();
                }


                
                
                for (int p = 0; p < type.Methods.Count; p++)
                {
                    MethodDefinition methodDefinition = type.Methods[p];                   
                    foreach (var par in methodDefinition.Parameters)
                    {
                        if (type.BaseType != null)
                        {
                            if (type.BaseType.Name == "MonoBehaviour")
                            {
                                if (Program.methods.Contains(methodDefinition.Name))
                                {
                                    continue;
                                }
                                else
                                {
                                    methodDefinition.Name = GetRandomString();
                                }
                            }
                            
                        }
                        else
                        {
                            methodDefinition.Name = GetRandomString();
                        }
                        par.Name = GetRandomString();
                    }
                }
            }
        }

        /// <summary>
        /// Ghi lại những chỉnh sửa tệp dll 
        /// </summary>
        /// <param name="assembly">Lớp thông tin tệp dll </param>
        /// <param name="file">Đường dẫn thư mục</param>
        /// <exception cref="FileNotFoundException">Không tìm thấy file dll vừa sửa xong</exception>
        static void WriteDLL(AssemblyDefinition assembly, string file)
        {
            using (Stream stream = new FileInfo(file).Open(FileMode.OpenOrCreate))
            {

                stream.Close();
                assembly.Write(file);
            }
        }
        /// <summary>
        /// Đọc tệp DLL sử dụng using và memory để IOException không bị xảy ra 
        /// </summary>
        /// <param name="file">Đường dẫn tệp</param>
        /// <returns>Lớp chưa tất cả thông tin của tệp DLL </returns>
        static AssemblyDefinition ReadDLLFile(string file)
        {
            AssemblyDefinition assembly = null;
            using (Stream stream1 = File.OpenRead(file))
            {
                MemoryStream memory = new MemoryStream();
                stream1.CopyTo(memory);
                stream1.Close();
                MemoryStream memory2 = new MemoryStream(memory.ToArray());
                assembly = AssemblyDefinition.ReadAssembly(memory2);
            }
            return assembly;
        }
    }
}
