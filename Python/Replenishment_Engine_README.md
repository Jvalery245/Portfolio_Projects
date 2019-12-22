# Replenishment - Parsing Engine

Project Focus:
This project is an exercise in do, do while, and recursion principals. Within a retail setting that uses a replenishment method called "Top to Maximum" their must be a lower and upper limit integer set for each stock keeping unit based on sales, turns of inventory, and costing analysis per store, per category, per planogram, and per vendor. This program is the final engine that parses the logic feed back from a tensor flow algorithm that generates a suggested minimum and maximum stocking level. The efficiency of the program is based on two restrictions, the amount of recursions that are with each factorial and the speed of the logical algorithm that produces the values. The next version of the program will create a live feedback mechanism that will give real-time suggestions for minimum and maximum stocking levels based on current data provided from sales, units purchased, and incurred costs so as to factor in profitability margins with suggested stocking levels.

Facts Concerning Replenishment:
The "Top to Max" feature being used in the project is an algorithm that takes the delta between the minimum and maximum values of set parameters and then compairs it to the smallest shippable unit or UOM of a product code. If the delta between the currect stocking level equals one pack size and the stocking goes below the minimum stocking level, then the system generates a purchase order automatically, if not the record is ignored until the new process is run. The validation of these minimum and maximum values are controlled and suggested based on logic algorithms that optimize supply chain flow and profitability.

Formula Logic and Example of Usage:
1. if Current_Stocking_Level < Min:
 1.-Create a suggestion logic
Current_Stocking_Level - Max = Order_Unit_Suggestion
if Order_Unit_Suggestion >= Pack_Size:
-- Commit to background process and generate an orders
else:
--Do not Generate an order because it does not equal a shippable unit 
else:
-Do not create a suggestion
 
Tools Used:
Python
Pandas

Production Implementation Date:
09/2019
