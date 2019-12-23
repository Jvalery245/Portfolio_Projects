# PDF Processing Module

**Project Purpose**
    The warehouse is constantly sending out packages to the stores for orders that are allocations from the merchandising team or simple requests for supplies. The requests come into the warehouse from two sub-systems that do not track vital data that is necessary for visibility to upper management. This module takes the documents produced by the warehouse and produces a refined document and parsed pdf documents per store, per pick list ,and per shipment. When compared to the cost of purchasing a document scanning product and the scalability of implementation, it was determined that having a customizable alternative was better that purchasing a pre-built software. The data collected then drives a model that can be used to track metrics for shipments and efficiency of the warehouse. The engine is controlled with a sub-process scheduler that runs the program every five minutes once started form a batch file put on a windows scheduled process.

**Project Logic Chart**
1) Call out to the network folder and search for an files with the suffix '.pdf'
2) If a file is found, open parsing engine, If not then wait another 5 minutes, and re-run the process.
3) If a file is found, scan the first two pages to determine if the file is from the warehouse. If it is determined to not be, the file is left in the staging process and the program waits another 5 minutes. If it is, the program then parses the front and back pages of the document looking for relevant data concerning warehouse productivity and metrics. The regular expressions are created to locate the Transaction Number, Pick Ticket Number, Location Number, UPC/FedEx tracking number, and the AIRS ident Number. A new file is then put into the shared network folder with the directory that is created from the current date the file was scanned and a sub-directory with the original file stored.

**Technologies Used**
Python
Regular Expressions (RE)
PyPDF2 - Framework

**Production Implementation Date**
11/2019
