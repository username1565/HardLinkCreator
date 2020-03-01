using System;
using System.Runtime.InteropServices;	//need to using DllImport
using System.Linq;

namespace HardLinkCreator
{
	/**
		Usage: HardLinkCreator [source_pathway] [destination_pathway]
		This program accept two pathway, and create hardlink in destination pathway of not existing file
		from source pathway of existing file.
		Check pathways, and create hardlink, or just return error, if pathways is invalid or source file does not exists.
	*/

    class HardLinkCreator
    {
		/**
			//method need to create hardlink
		*/
		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode )]
		static extern bool CreateHardLink(
			string destionation_filepath_for_create_hardlink,		//where need to create hardlink
			string source_filepath_of_existing_file,				//from this existing file
			System.IntPtr SecurityAttributes						//param to create hardlink
		);

		/**
			//method to validate filepath
			Return false if something wrong, or return true, if filepath is valid.
			Check directory and create directory if this does not exists.
			Check is file exists if file may be exists = true, and return false, else if not may be exists, then do not return false, and continue.
			Commented Console.WriteLine(Number) can be uncommented, to see where this method return false, and what is wrong there.
		*/
		public static bool IsValidFileName(string name, bool may_be_exist = false, bool allowRelativePaths = true) {
			//Disabling: warning CS1717: Assignment made to same variable; did you mean to assign something else?
			//[Yes, need to escape the slashes in file pathway.]
			#pragma warning disable 1717
			name = @name;					//@ means escape "\" to "\\" 
			#pragma warning restore 1717
			
			if(string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name)){
				//Console.WriteLine("1");
				return false;
			}				//return false if whitespace, null or empty string
			if(name.Length > 1 && name[1] == ':') {
				if(name.Length < 4 || name.ToLower()[0] < 'a' || name.ToLower()[0] > 'z' || name[2] != '\\'){
					//Console.WriteLine("2");
					return false;				//return false in this all cases
				}
				name = name.Substring(3);
			}
			if(name.StartsWith("\\\\")){
				name = name.Substring(1);
			}
			if(			name.EndsWith("\\")
					||	!name.Trim().Equals(name)
					||	name.Contains("\\\\")
					||	name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars().Where(x=>x!='\\').ToArray()) >= 0
					||	name == "con"
			){
				//Console.WriteLine("3");
				return false;	//return false in this all cases
			}
			
			//Try open the file and try to read it, then close, to recognize is this file exists?
			try												//try 
			{												//to open file, and read it, then close
				System.IO.File.OpenRead(name).Close();		//if success, continue
			}
			catch (ArgumentException) {						//else if invalid argument exception
				//Console.WriteLine("4");
				return false;								//return false
			}
			catch (											//else if file not found
				Exception /*ex //need to show exception*/	//uncomment it to show console.log exception
			) {
				if(may_be_exist == true){					//if file may be exists
					if(!System.IO.Directory.Exists(name))		//check is this exists again, and if not exists
					{
						//Console.WriteLine("5, ex: "+ex);
						return false;							//return false
					}
				}
			}
			
			//Try to check directory name from the fullpath of the saved file
			try
			{
				string dir = System.IO.Path.GetDirectoryName(name);	//get directory name and extract directory from full path of the file
				if((string.IsNullOrEmpty(dir) == false) && (System.IO.Directory.Exists(dir) == false)){ //if this not null and if this does not exists
					//Console.WriteLine("dir: "+dir+", System.IO.Directory.Exists(dir): "+System.IO.Directory.Exists(dir));	//show it
					try{												//try
						System.IO.Directory.CreateDirectory(dir);		//to create this directory, and if ok - continue
					}
					catch{												//else, if error
						//Console.WriteLine("6");						
						return false;										//return false
					}
				}
			}
			catch														//if cann't get directory name
			{
				//Console.WriteLine("7");
				return false;											//return false
			}

			//check is directory OK
			try
			{
				if (allowRelativePaths){								//if relative pathways allowed
					if(System.IO.Path.IsPathRooted(System.IO.Path.GetFullPath(name)) == false){	//get full path and check is this rooted, and if no...
						//Console.WriteLine("8");	
						return false;															//return false
					};
				}
				else{													//if relative pathways not allowed
					if(string.IsNullOrEmpty((System.IO.Path.GetPathRoot(name)).Trim(new char[] { '\\', '/' }))){	//get root path, trim slashes, check is null or empty, and if empty
						//Console.WriteLine("9");
						return false;																				//return false
					}
				}
			}
			catch(
				Exception	/*ex // this need to be uncommented, to show exception in Console.WriteLine*/
			)
			{
				//Console.WriteLine("10 "+ex);
				return false;
			}
			
			return true;	//else if all ok, and done - return true
		}

		public static void Create_Hardlink(string[] args){
			//	Show arguments:
			//Console.WriteLine("args.Length: "+args.Length);
			//for(int i = 0; i<args.Length; i++){
			//	Console.WriteLine("args["+i+"]: "+args[i]);
			//}
			
			if(args.Length<2){						//if not enough arguments specified
				Console.WriteLine(		//show usage
									"Usage: HardLinkCreator [source path] [destination path]\n"+
									"Error! Not enough arguments specified! args.Length: "+args.Length+", (args.Length<2): "+(args.Length<2)+"\n"+
									"Minimum array of arguments length must to be 2 [source path] and [destination path]"
				);
				return;					//and return
			}//else:
			
			string source_filepath_of_existing_file 					= 	args[0];	//source file path	
			string destionation_filepath_for_create_hardlink	 		= 	args[1];	//destination file path
			
			if(!IsValidFileName(source_filepath_of_existing_file, true)){ 	//this file may be exist (true), and if filepath is invalid - show this
				Console.WriteLine("Invalid filepath! Was specified: "+source_filepath_of_existing_file+", "+source_filepath_of_existing_file.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()));
				return;														//and return
			}
			if(!System.IO.File.Exists(source_filepath_of_existing_file)){	//then, check is file exists? if does not exists
				Console.WriteLine("Source file not found!");					//show this;
				return;															//and return;
			}else{															//else, if file is found, try to create hardlink
				Console.WriteLine("Source file found!");						//just show this
				if(!IsValidFileName(destionation_filepath_for_create_hardlink, false)){ 									//this file still not created, check filepath of this, and if invalid
					Console.WriteLine("Invalid filepath! Was specified: "+destionation_filepath_for_create_hardlink);		//show this
					return;																									//and return false
				}else{															//else, if valid
					Console.WriteLine("Try to create hardlink for file "+source_filepath_of_existing_file+" in file "+destionation_filepath_for_create_hardlink);	//show this, and try to create hardlink
					try{																											//try
						CreateHardLink(@destionation_filepath_for_create_hardlink, @source_filepath_of_existing_file, IntPtr.Zero);		//to create hardlink
						Console.WriteLine("Done!");																						//and after creating, show done
						return;																											//and return
					}catch(Exception ex){																							//else	
						Console.WriteLine("Error. Throw exception: "+ex);																//show throw exception
						return;																											//and return
					}
				}
			}
		}
		
        static void Main(string[] args)	//main program accept command-line arguments
        {
			Create_Hardlink(args);		//Just try to create hardlink with this arguments
			//Console.ReadKey();			//and wait press any key.	//or not
			//return;							//and return;
        }
    }
}