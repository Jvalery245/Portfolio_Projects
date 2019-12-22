import numpy as np
import pandas as pd
import os
import time
import sys

"""
This project is an exercise in do, do while, and recursion principals. Within a retail setting that uses a replenishment method called "Top to Maximum" their must be a lower and upper limit integer set for each stock keeping unit based on sales, turns of inventory, and costing analysis per store, per category, per planogram, and per vendor. This program is the final engine that parses the logic feed back from a tensor flow algorithm that generates a suggested minimum and maximum stocking level. The efficiency of the program is based on two restrictions, the amount of recursions that are with each factorial and the speed of the logical algorithm that produces the values. The next version of the program will create a live feedback mechanism that will give real-time suggestions for minimum and maximum stocking levels based on current data provided from sales, units purchased, and incurred costs so as to factor in profitability margins with suggested stocking levels.
"""


class MinandMax():
    """
    Validation of minimum and maximum validation:
    Pack size and Inventory Count variable can be added for validation of order generation but is removed for file rastering
    Result: Parsed workbook foreach module/planogram and combined data. Creating audit trail for each store and each Planogram/Grade item.
    """

    def __init__(self, SKU, minimum, maximum):
        self.minimum = minimum
        self.maximum = maximum

    def __str__(self):
        return f"The current SKU is {self.SKU}. The SKU's min is {self.minimum} and max is {self.maximum}"

    def MinValidation(self):
        minvalue = int(self.minimum)
        maxvalue = int(self.maximum)
        if minvalue < maxvalue:
            result = "Minimum validation successful on SKU"
            return result
        elif minvalue > maxvalue:
            result = "Alert!: Minimum value is greater than maximum value"
            return result
        elif minvalue == 0:
            result = "Caution: Minimum value set to zero (0)"
            return result
        elif minvalue == maxvalue:
            result = "Caution: Minimum and Maximum values are the same"
            return result
        else:
            result = "Please review minimum parameter on SKU"
            return result

    def MaxValidation(self):
        minvalue = int(self.minimum)
        maxvalue = int(self.maximum)
        if maxvalue > minvalue:
            result = "Maximum validation successful on SKU"
            return result
        elif maxvalue == minvalue:
            result = "Caution: The min and max values are set the same"
            return result
        elif maxvalue == 0:
            result = "Caution: Maximum value set to zero(0)"
            return result
        elif maxvalue < minvalue:
            result = "Alert: Minimum value is greater than maximum value"
            return result
        else:
            result = "Please review maximum parameter on SKU. The value is not a valid integer"
            return result


class Vendor_Information:
    def __init__(self, StoreGrade):
        self.StoreGrade = StoreGrade

    def StoreDataCleaning(self, BASE_DIR):
        systempath = BASE_DIR
        vendor_data_path = "Store_Files\\Raw_Store_Data_New.xlsx"
        vendorSKUfilepath = os.path.join(systempath, vendor_data_path)
        if os.path.exists(vendorSKUfilepath) == True:
            sheet_Names = pd.read_excel(vendorSKUfilepath, None)
            print(sheet_Names.keys())
            usermarket = str(self.StoreGrade)

            # Reading Excel Data from Marketing Department
            vendor_data = pd.read_excel(vendorSKUfilepath, sheet_name=usermarket, converters={"ProductCode": lambda x: str(x), "MaxStock": lambda x: str(
                x), "MinStock": lambda x: str(x), "StoreCode": lambda x: str(x)})  # Reading vendor_data Class from Marketing Spreadsheet
            memory = vendor_data.info(memory_usage='deep')
            print(memory)
            store_active = vendor_data

            SKU_store_active_list = store_active["ProductCode"].to_list()
            Min_store_active_list = store_active["MinStock"].to_list()
            Max_store_active_list = store_active["MaxStock"].to_list()

            validation_report = pd.DataFrame()

            """
            Validating the conditions of min and max values based on MI-9 documentation
            """
            for SKU, Min, Max in zip(SKU_store_active_list, Min_store_active_list, Max_store_active_list):
                print(f"Processing SKU {SKU}")
                item_validation = MinandMax(str(SKU), int(Min), int(Max))
                maxresult = item_validation.MaxValidation()
                minresult = item_validation.MinValidation()
                print(
                    f"Validation results for SKU {SKU}:\n{maxresult}\n{minresult}")
                dataresults = {"Module": str(usermarket),
                               "SKU": str(SKU), "Min": int(Min), "Max": int(Max),
                               "Max_Validation": str(maxresult), "Min_Validation": str(minresult), "Store_Band": str(self.StoreGrade)}
                report = pd.DataFrame(data=dataresults, index=["SKU"])
                validation_report = validation_report.append(report)
            print("Saving min and max audit log......")
            validation_save_path = os.path.join(
                systempath, "Min_Max_Audit\\Min_Max_Validation{}.xlsx".format(str(usermarket)))
            validation_report.to_excel(validation_save_path, index=False)
            print("Min and max audit log saved......")
            print(store_active.info())
            print('There were {} total records generated for {} stores'.format(
                len(store_active), str(usermarket)))
            return store_active
        else:
            print("Marketing department data for SKUs not found. Please aquire data and try again\nReturning to main menu...")


def Main(Program_Main_Dir):
    """
    Module for cleaning and parsing all the files from the marketing department
    """
    systempath = Program_Main_Dir
    vendor_data_path = "Store_Files\\Raw_Store_Data_New.xlsx"
    datafilepath = os.path.join(systempath, vendor_data_path)
    combined_data = pd.DataFrame()
    mass_output = pd.DataFrame()
    verisheetname = pd.ExcelFile(datafilepath)
    modules = verisheetname.sheet_names
    print(modules)

    for mod in modules:
        Store_Modules = Vendor_Information(mod)
        store_Data = Store_Modules.StoreDataCleaning(Program_Main_Dir)
        store_Data["StoreCode"] = store_Data["StoreCode"].apply(
            lambda x: "{0:0>4}".format(x))
        store_file_Loc_Dir = os.path.join(systempath, "Audit_Parsed_Data\\Vendor_Parsed_Data_Store_{}.xlsx".format(
            str(mod)))

        store_Data.to_excel(
            store_file_Loc_Dir, sheet_name="{}".format(str(mod)), index=False)

        print(f"Assigning hard set values to {mod}")
        store_Data = store_Data[[
            "ProductCode", "PreferredVendorCode", "MinStock", "MaxStock", "StoreCode"]]
        store_Data = store_Data.assign(Status='0')  # tinyint
        store_Data = store_Data.assign(PoPlacementMethod='4')  # tinyint
        store_Data = store_Data.assign(ReplenMethodCode='03')  # tinyint

        currenttime = time.strftime("%Y%m%d-%H%M%S")

        final_filecsv = 'MI9_Upload\\MMS_StoreProducts_{}{}.txt'.format(
            str(mod), currenttime)
        final_fileexcel = 'MI9_Upload\\MMS_StoreProducts_{}{}.xlsx'.format(
            str(mod), currenttime)
        final_filepathexcel = os.path.join(systempath, final_fileexcel)
        final_filepathcsv = os.path.join(systempath, final_filecsv)
        print(f"Processing module {mod} excel output")
        store_Data.to_excel(final_filepathexcel, index=False)
        print(f"Processing module {mod} tab deliminated output")
        store_Data.to_csv(final_filepathcsv, sep="\t",
                          float_format=str, header=True, index=False)
        combined_data = combined_data.append(store_Data)
        mass_output = mass_output.append(store_Data)

    print('There were {} total records generated for upload'.format(len(combined_data)))
    total_vendorfile = 'Audit_Parced_Data\\Combined_Vendor_Parsed_Data.xlsx'
    total_vendorfilepath = os.path.join(systempath, total_vendorfile)
    combined_data.to_excel(total_vendorfilepath,
                           sheet_name='Parsed_Data', index=False)
    mass_output.to_csv("All_Records.txt", sep="\t",
                       float_format=str, header=True, index=False)
    mass_output.to_excel("All_Records.xlsx", index=False)


if __name__ == "__main__":
    print("Entering UPC/Vendor parsing module")
    print("Running UPC parsing sequence")
    Main()
