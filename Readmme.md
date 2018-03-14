# How to Build

* Clone the repository @ https://github.com/ljdongre/HospitalSimulator.git
* Navigate to folder **'HospitalSimulator'** in a command prompt.
* Execute 'Build.bat'
	* The build creates a **'DeploymentFolder'** Folder with sub folders 'Client' and 'server'
	* The sub folder 'client' contains test client **'HospitalSimulatorClient.exe'**
	* The sub folder 'Server' contains WCF based service **'HospitalSimulator.Host.exe'**


# How to Run

* Open 2 command windows.
* In each command window navigate to  **HospitalSimulator\DeploymentFolder**
* In one of the command window navigate to **'Client'** sub folder
* In the other command window navigate to **'Server'** sub folder.
* In 'Server' command window execute : **HospitalSimulator.Host.exe** command.
	* This will start the WCF service as a Console application.
	* To stop the service enter CTRL+C in the command window.
* In the 'Client' Command window, execute **'HospitalSimulatorClient.exe'**
* This will start a test client which will allow one to exercise the Server.


# How to Test

* Navigate to the **'Client'** Command window
	* From the menu select
		* 1: to register a patient. The system will ask the pre-requisite information necessary to create a patient.
		* 2: to query the status of registration. The request initiated in 1, post a command to the system to book the next available reservation slot. The query provides the status of the request.
		* 3: Allows one to get the registered patients list.
		* 4: Allows one to get the scheduled consultations list
		* 5: Allows one to stress test the application;
			* As part of stress testing, client will initiate 10 threads. In each of these threads, one of the above mentioned commands will be randomly executed with randomised data.
			* The system logs the interaction to a file available in the local folder.
			* At present one can review this file to identify issues.

# Interface Definition 

Hospital Simulator has been implemented as a plain WCF service providing the following interface:

* RegisterPatient: Registers a Patient
* IsRegistrationSuccessful - Return a Tuple indicating a status and Consultaion record
	* Consultation record contents are valid when the status is true.

* RegsiteredPatients : Retrieves a list of Registered patients.
* ScheduledConsultations : Retrieves a list of Scheduled consultations

For details refer to code available in **HospitalSimulatorService.Contract** sub folder.

## Obvious Missing Functionality

* Appointment cancellation support
	* Although the underlying infrastructure does support cancellation, the functionality is not exposed at the user level.

#Design

## Resource management
Each resource managed by service implements IResource interface. This interface provides the following functionality:

* Query for a available date range
* Reserve a date
* Un-reserve a date
* ID of the resource

Abstract class Resource implements most of the functionality. It may have been better to implement the functions as Virtual, thus allowing the sub-classes to override them and implement there own version of appointment scheduling. At present I can't think about such a scheduling requirement.

System defines the following resources.
* Doctor
* Treatment Room

Each resource stores its own schedule as appointment map. An appointment map is 24 byte storage allowing one to schedule appointments 6 month in advance. Each byte is mapped to a week with appointments scheduled from Monday to Friday. The week is mapped to the byte from MSB (most significant Bit) to LST (least significant bit). Each byte at preent leaves off 3 bits. Thus the byte array is initiatlized with byte value 0x& and uses Monday mask as 0x80.

At present resources are embedded in the code. But, one could read them from a file or a any other persistence source.

## Resource Allocation
Resource Allocation is handled by **ResourceAllocator**. This is at present implemented as an active object and static to the service. The main thread process the requests posted to its queue. At present the queue is plain FIFO and does not implement any priority scheme. The requests are then processed to identify valid resources defined  by its rules. Each Resource has an associated resource rule, which decides its suitability based on the patient condition. The rule logic is embedded in the code and could be isolated to a rule based system with its own rule DSL.
The identified resources are then queried for their availability range which is then used to create the actual schedule date.

# Additional work that was not implemented

* cancel functionality is not exposed.
* Rules are defined in code and could be isolated to a DSL
* The resource query is single threaded and could be paralleled to improve performance, if needed. 
* No performance measurements were done to reach the above conclusion.




