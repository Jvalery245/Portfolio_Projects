#!/usr/bin/env python3

from Shipping_PDF import PDF_Processing
import os
import datetime
import glob
import sys
import datetime
import time
from apscheduler.schedulers.background import BlockingScheduler

"""
#============================================================================================================================================\n
PDF Pasring Engine for Warehouse Store orders and Store AIRS orders\n
#============================================================================================================================================\n
"""


def Main():
    # The networrk folder that hosues all scans produced by all warehouses.
    original_pdf_document_folder = "C:..........."

    pdf_documents = glob.glob(original_pdf_document_folder+"/*.pdf")

    num_of_files = len(pdf_documents)
    current_time = datetime.datetime.now()
    strcurrent_time = current_time.strftime("%B%d")

    StoreOrdervendor_folder = "C:................"+strcurrent_time

    PDF_Processing_Choice = "1"

    if PDF_Processing_Choice == "1":
        if num_of_files > 0:
            print("Processing store order scans")

            if os.path.exists(StoreOrdervendor_folder):
                WHPDFS = PDF_Processing.PDF_Document_Processing(
                    StoreOrdervendor_folder, pdf_documents)
                WHPDFS.PDF_Page_Parsing()
                print("Processing complete. Have a lovely day!")
            else:
                os.mkdir(StoreOrdervendor_folder)
                WHPDFS = PDF_Processing.PDF_Document_Processing(
                    StoreOrdervendor_folder, pdf_documents)
                WHPDFS.PDF_Page_Parsing()
                print("Processing complete. Have a lovely day!")
        if num_of_files == 0:
            print("No new files in processing repository. Have a lovely day!")

    elif PDF_Processing_Choice == "Exit":
        print("Goodbye and have a lovely day!")
        sys.exit()


if __name__ == "__main__":
    EveryMinuteProcess = BlockingScheduler()
    EveryMinuteProcess.add_job(Main, "interval", minutes=5)
    startcurrenttime = datetime.datetime.now()
    startdate = startcurrenttime.strftime("%m/%d/%Y")
    starttime = startcurrenttime.strftime("%H:%M")
    print(f"Process started at {starttime} on {startdate}")
    print("Starting main engine for PDF parsing.")
    EveryMinuteProcess.start()
