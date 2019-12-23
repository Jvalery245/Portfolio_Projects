#!/usr/bin/env python3

from PyPDF2 import PdfFileReader, PdfFileWriter
import re
import os
import shutil


class PDF_Rotation():
    """
    Class to rotate the PDF page to find the text:
    page = PyPDF2 instance of bitwise data object of a PDF file 
    """

    def __init__(self, page):
        self.page = page

    def Page_Rotation(self):
        page = self.page
        newpage = page.rotateClockwise(90)
        return newpage


class Data_Parsing:
    """
    Class instance of a PDF object to be parsed for information.
    Variables:
    page = PyPDF2 Object: front page instance of PDF file
    backpage = PyPDF2 Object: back page instance of PDF file
    vendor_folder = str: that is the directory to the file stored location after data extraction
    """

    def __init__(self, page, backpage, vendor_folder):
        self.page = page
        self.backpage = backpage
        self.vendor_folder = vendor_folder

    def File_Orientation_Found(self):
        # Regular expression used to finf relavent information on the PDF document. In the Warehouse
        # the information needed for fullfillment of orders is the Transaction Numer, Pick Ticket Number,
        # Location Number, UPC/FedEx tracking number, and the AIRS ident Number.
        TransNumPat = "[Tt]{1}[Rr]{1}[Aa]{1}[Nn]{1}[Ss]{1}[Ff]{1}[Ee]{1}[Rr]{1}[#]{1}[:]{1}\s[C]{1}[0-9]{6}"
        PickNumPat = "[Pp]{1}[Ii]{1}[Cc]{1}[Kk]{1}\s[Ll]{1}[Ii]{1}[Ss]{1}[Tt]{1}[:]{1}\s[0-9]{0,6}"
        LocationPat = "[Ll]{1}[Oo]{1}[Cc]{1}[Aa]{1}[Tt]{1}[Ii]{1}[Oo]{1}[Nn]{1}[:]{1}\s[0-9]{1,6}"
        TrackingNumPat = "[Tt]{1}[Rr]{1}[Aa]{1}[Cc]{1}[Kk]{1}[Ii]{1}[Nn]{1}[Gg]{1}[#]{1}\s{1}[0-9A-Z]{12,25}"
        identpat = "[0-9]{1,3}[,]{1}[0-9]{1,3}"

        # Processing one page
        pagefile = self.page
        pagetext = pagefile.extractText()
        backpage = self.backpage
        backpagetext = backpage.extractText()

        print(pagetext)

        print("Values found are:")
        filenamejoin = []

        # Setting regex patterns and searching return string on front page
        # If a match is found the match if appened to the new file name list
        if re.search(TransNumPat, pagetext) != None:
            TransNumFind = re.search(TransNumPat, pagetext)
            TransNum = TransNumFind.group()
            print(f"Property found: {TransNum}")
            filenamejoin.append(TransNum)
        else:
            print("Alert: No transfer number found on front page")

        if re.search(PickNumPat, pagetext) != None:
            PickNumFind = re.search(PickNumPat, pagetext)
            PickNum = PickNumFind.group()
            print(f"Property found: {PickNum}")
            filenamejoin.append(PickNum)
        else:
            print("Alert: No pick number found on front page")

        if re.search(LocationPat, pagetext) != None:
            LocationFind = re.search(LocationPat, pagetext)
            Location = LocationFind.group()
            print(f"Property found: {Location}")
            filenamejoin.append(Location)
        else:
            print("Alert: No location found on front page")

        if re.search(TrackingNumPat, pagetext) != None:
            TrackingFind = re.search(TrackingNumPat, pagetext)
            TrackingNum = TrackingFind.group()
            print(f"Property found: {TrackingNum}")
            filenamejoin.append(TrackingNum)
        else:
            print("Alert: No tracking number found on front page")

        if re.search(identpat, pagetext) != None:
            IDNumFind = re.search(identpat, pagetext)
            IDNum = IDNumFind.group()
            print(IDNum)
            filenamejoin.append(IDNum)
        else:
            print("Alert: No ID number found on front page")

        # Setting regex patterns and searching return string of back page
        # If a match is found the match if appened to the new file name list
        if re.search(TransNumPat, backpagetext) != None:
            TransNumFind = re.search(TransNumPat, backpagetext)
            TransNum = TransNumFind.group()
            print(f"Property found: {TransNum}")
            filenamejoin.append(TransNum)
        else:
            print("Alert: No transfer number found on back page")

        if re.search(PickNumPat, backpagetext) != None:
            PickNumFind = re.search(PickNumPat, backpagetext)
            PickNum = PickNumFind.group()
            print(f"Property found: {PickNum}")
            filenamejoin.append(PickNum)
        else:
            print("Alert: No pick number found on back page")

        if re.search(LocationPat, backpagetext) != None:
            LocationFind = re.search(LocationPat, backpagetext)
            Location = LocationFind.group()
            print(f"Property found: {Location}")
            filenamejoin.append(Location)
        else:
            print("Alert: No location found on back page")

        if re.search(TrackingNumPat, backpagetext) != None:
            TrackingFind = re.search(TrackingNumPat, backpagetext)
            TrackingNum = TrackingFind.group()
            print(f"Property found: {TrackingNum}")
            filenamejoin.append(TrackingNum)
        else:
            print("Alert: No tracking number found on back page")

        if re.search(identpat, backpagetext) != None:
            IDNumFind = re.search(identpat, backpagetext)
            IDNum = IDNumFind.group()
            print(IDNum)
            filenamejoin.append(IDNum)
        else:
            print("Alert: No ID number found on back page")

        # After the front and back pages of the first and second page are parsed, the new file is saved using the information parsed.
        # with an underscore between each value.
        print(f"{len(filenamejoin)} characteristics found on page")
        outputfilename = "_".join(filenamejoin)
        outputfilename = outputfilename.replace(" ", "")
        outputfilename = outputfilename.replace(":", "_")
        outputfilename = self.vendor_folder+"/"+outputfilename+".pdf"
        new_PDF = PdfFileWriter()
        new_PDF.addPage(pagefile)
        new_PDF.addPage(backpage)
        print(outputfilename)
        new_PDF.write(open(str(outputfilename), "wb"))


class PDF_Document_Processing:
    def __init__(self, vendor_folder, pdf_documents):
        self.vendor_folder = vendor_folder
        self.pdf_documents = pdf_documents

    def PDF_Page_Parsing(self):
        for PDF_doc in self.pdf_documents:
            # An audit log generation is completed for all processes to quickly find out the status of shipemnts that
            # have left the warehouses.
            audit_file_location = "C:........Log_Files/AuditLogPDFProcessing.txt"
            with open(audit_file_location, "a") as logfile:
                original_pdf_document = PDF_doc
                print(f"Processing document{PDF_doc}")

                # FIRST Check of any qualifying information:
                # Regular expression used to find relavent information on the PDF document. In the Warehouse
                # the information needed for fullfillment of orders is the Transaction Numer, Pick Ticket Number,
                # Location Number, UPC/FedEx tracking number, and the AIRS ident Number.
                TransNumPat = "[Tt]{1}[Rr]{1}[Aa]{1}[Nn]{1}[Ss]{1}[Ff]{1}[Ee]{1}[Rr]{1}[#]{1}[:]{1}\s[C]{1}[0-9]{6}"
                PickNumPat = "[Pp]{1}[Ii]{1}[Cc]{1}[Kk]{1}\s[Ll]{1}[Ii]{1}[Ss]{1}[Tt]{1}[:]{1}\s[0-9]{0,6}"
                LocationPat = "[Ll]{1}[Oo]{1}[Cc]{1}[Aa]{1}[Tt]{1}[Ii]{1}[Oo]{1}[Nn]{1}[:]{1}\s[0-9]{1,6}"
                TrackingNumPat = "[Tt]{1}[Rr]{1}[Aa]{1}[Cc]{1}[Kk]{1}[Ii]{1}[Nn]{1}[Gg]{1}[#]{1}\s{1}[0-9A-Z]{12,25}"
                identpat = "[0-9]{1,3}[,]{1}[0-9]{1,3}"

                with open(original_pdf_document, "rb") as original:
                    # Checking the first page for the "Pick number"
                    """
                    If the first page has a pick number the script will continue. If not then the process moves to the next record
                    """
                    pdf_file = PdfFileReader(original)
                    pages = pdf_file.getNumPages()
                    for page in range(0, pages, 2):
                        logfile.write(
                            f"Processed document{PDF_doc}\nPage number: {page}\n")
                        verpage = pdf_file.getPage(page)
                        backpage = pdf_file.getPage(page+1)
                        pagetext = verpage.extractText()

                        #   Rotating image 360 degrees, in 90 dgree increments, to catch the correct orientation of text for parsing:
                        #   Original Orientation
                        print("Searching original orientation")
                        if re.search(PickNumPat, pagetext) == None and re.search(identpat, pagetext) == None and re.search(TrackingNumPat, pagetext) == None:
                            print(
                                f"No data found in original orientation. Page {page}\n")
                            logfile.write(
                                f"No data found in original orientation. Page {page}\n")
                            rotate = PDF_Rotation(verpage)
                            rotateOne = rotate.Page_Rotation()
                            pagetextone = rotateOne.extractText()

                        elif re.search(PickNumPat, pagetext) != None:
                            print(
                                f"Pick number data found in original orientation: Page {page}\n")
                            logfile.write(
                                f"Pick number data found in original orientation: Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                verpage, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(identpat, pagetext) != None:
                            print(
                                f"Legacy number data found in original orientation: Page {page}\n")
                            logfile.write(
                                f"Legacy number data found in original orientation: Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                verpage, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(TransNumPat, pagetext) != None:
                            print(
                                f"Transfer number data found in original orientation: Page {page}")
                            logfile.write(
                                f"Transfer number data found in original orientation: Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                verpage, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(TrackingNumPat, pagetext) != None:
                            print(
                                f"Tracking number data found in original orientation: Page {page}\n")
                            logfile.write(
                                f"Tracking number data found in original orientation: Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                verpage, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue

                        print("Rotating image once")
                        # Second Rotation
                        if re.search(PickNumPat, pagetextone) == None and re.search(identpat, pagetextone) == None and re.search(TrackingNumPat, pagetextone) == None:
                            print(
                                f"No data found after rotaion one. Page {page}\n")
                            logfile.write(
                                f"No data found after rotaion one. Page {page}\n")
                            rotate = PDF_Rotation(rotateOne)
                            rotateTwo = rotate.Page_Rotation()
                            pagetexttwo = rotateTwo.extractText()

                        elif re.search(PickNumPat, pagetextone) != None:
                            print(
                                f"Pick number data found after rotaion one. Page {page}\n")
                            logfile.write(
                                f"Pick number data found after rotaion one. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateOne, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(identpat, pagetextone) != None:
                            print(
                                f"Legacy number data found after rotaion one. Page {page}\n")
                            logfile.write(
                                f"Legacy number data found after rotaion one. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateOne, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(TransNumPat, pagetextone) != None:
                            print(
                                f"Transfer number data found after rotaion one. Page {page}\n")
                            logfile.write(
                                f"Transfer number data found after rotaion one. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateOne, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(TrackingNumPat, pagetextone) != None:
                            print(
                                f"Tracking number data found after one rotation: Page {page}\n")
                            logfile.write(
                                f"Tracking number data found after one rotation: Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                verpage, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue

                        print("Rotating image twice")
                        # Third Rotation
                        if re.search(PickNumPat, pagetexttwo) == None and re.search(identpat, pagetexttwo) == None and re.search(TrackingNumPat, pagetexttwo) == None:
                            print(
                                f"No data found after rotaion two. Page {page}\n")
                            logfile.write(
                                f"No data found after rotaion two. Page {page}\n")
                            rotate = PDF_Rotation(rotateTwo)
                            rotateThree = rotate.Page_Rotation()
                            pagetextthree = rotateThree.extractText()

                        elif re.search(PickNumPat, pagetexttwo) != None:
                            print(
                                f"Pick number data found after rotaion two. Page {page}\n")
                            logfile.write(
                                f"Pick number data found after rotaion two. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateTwo, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(identpat, pagetexttwo) != None:
                            print(
                                f"Legacy number data found after rotaion two. Page {page}\n")
                            logfile.write(
                                f"Legacy number data found after rotaion two. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateTwo, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(TransNumPat, pagetexttwo) != None:
                            print(
                                f"Transfer number data found after rotaion two. Page {page}\n")
                            logfile.write(
                                f"Transfer number data found after rotaion two. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateTwo, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(TrackingNumPat, pagetexttwo) != None:
                            print(
                                f"Tracking number data found after rotation two: Page {page}\n")
                            logfile.write(
                                f"Tracking number data found after rotation two: Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                verpage, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue

                        print("Rotating image three times")
                        # Fourth Rotation
                        if re.search(PickNumPat, pagetextthree) == None and re.search(identpat, pagetextthree) == None and re.search(TrackingNumPat, pagetextthree) == None:
                            print(
                                f"No data found after rotaion three. Page {page}\n")
                            logfile.write(
                                f"No data found after rotaion three. Page {page}\n")
                            subprocessstatus = 0
                        elif re.search(PickNumPat, pagetextthree) != None:
                            print(
                                f"Pick number data found after rotaion three. Page {page}\n")
                            logfile.write(
                                f"Pick number data found after rotaion three. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateThree, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(identpat, pagetextthree) != None:
                            print(
                                f"Legacy number data found after rotaion three. Page {page}\n")
                            logfile.write(
                                f"Legacy number data found after rotaion three. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateThree, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(TransNumPat, pagetextthree) != None:
                            print(
                                f"Transfer number data found after rotaion three. Page {page}\n")
                            logfile.write(
                                f"Transfer number data found after rotaion three. Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                rotateThree, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                        elif re.search(TrackingNumPat, pagetextthree) != None:
                            print(
                                f"Tracking number data found after rotation three: Page {page}\n")
                            logfile.write(
                                f"Tracking number data found after rotation three: Page {page}\n")
                            subprocessstatus = 0
                            processedfile = Data_Parsing(
                                verpage, backpage, self.vendor_folder)
                            processedfile.File_Orientation_Found()
                            continue
                # If a 'subprocess' code of 0 is returned the document is not processed and not moved from the staging folder.
                # the file is assumeed to not be a relavent file for the warhouse and ignored. Else the file is processed and moved
                # to the original document is moved to the 'processed' folder
                if subprocessstatus == 0:
                    savelocation = self.vendor_folder+"/Processed/"
                    if os.path.exists(savelocation):
                        shutil.move(PDF_doc, savelocation)
                    else:
                        os.mkdir(savelocation)
                        shutil.move(PDF_doc, savelocation)
                elif subprocessstatus >= 1:
                    print(
                        "The scanned document page is not for the warehouse. Processing next page.")
                    print("File not moved due to not being processed.")
