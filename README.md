In my configuraition file (which let's forget i put here earlier) there is a 

-> "ConnectionStrings" defined which is responsible for the defaultConnection.

-> Inside of it, there is a Data Source so the server address and port(seperated by a coma).

-> there is a user id and password, which allow me to access the database.

-> Lastly, the "TrustServerCertificate" which can be either true or false, but in my case is defined as true 


In my solution I have just decided to keep it all in one project, yet different folders. There are two folders
1. Models - this one holds the classes which were created with EF, that I did not alter add all. Meaning they
   stayed the same as they were generated. (besides the MasterContex where I deleted the function as told by the tutor)
2. DTO - in here I kept all the DTOs for the requests that I had to create. They allowed me to properly get/put data as
   required in the documenation file. Their names determine for what they are used.
Of course the Project.cs is present in the solution, where everything connects. This division was implemented by me, becasue this
project is relatively small. Meaning I don't have to do any validation of data or any other "logic" structures. Because of that
in this case I thought it is appropriate to keep it as simple as possible. If any more complex functionalities would be required
then I would divide it into a more complex strucutre(like API, Repository, Logic, Models... just as we did in other projects)
