/***************************************************************
    * 
    * Nathan Kelderman
    * Professor Wolffe
    * CIS 452 - Simulated Paging Backend
    * 4/5/2018
    * 
    ***************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimulatedPaging
{
    /***************************************************************
     * 
     * This class handles loading from the file to the pcb and setting
     * up page tables, memory, and free frames lists when the next 
     * command is called. 
     * 
     ***************************************************************/

    class Backend
    {
        // stores the next commands waiting to be executed
        private List<Command> pcb;
        // stores the list of page tables for processes currently in memory
        private List<PageTable> pageTable;
        // stores the actual frames of memory
        private MFrame[] memory;
        // list that keeps track of all the free frames
        private List<int> freeFrames;

        //private List<Command> previousPcb;
        //private List<List<PageTable>> previousPageTables;
        //private List<MFrame[]> previousMemory;
        //private List<List<int>> previousFreeFrames;

        public Backend()
        {
            pcb = new List<Command>();
            pageTable = new List<PageTable>();
            memory = new MFrame[8];
            freeFrames = new List<int>();
            // initialize to 8 frames since thats how many we are given
            for (int x = 0; x < 8; x++)
                freeFrames.Add(x);

            //non-functioning code used for previous option
            //previousPcb = new List<Command>();
            //previousPageTables = new List<List<PageTable>>();
            //previousMemory = new List<MFrame[]>();
            //previousFreeFrames = new List<List<int>>();
        }
        
        /*******************************************************************
         * 
         * This method takes in a string as a filename and reads the contents 
         * of that file into the pcb, only if the contents match the standard 
         * format <processID> <command> or <processID> <textSize> <dataSize>.
         * 
         * returns the number of lines read if successful or a -1 if it couldn't
         * find the file.
         * 
         *******************************************************************/
        public int ReadFile(string filename)
        {
            int count = 0;
            Command c;
            // check if file exists
            if (File.Exists(filename))
            {
                // open a filestream for that file
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    // initialize a stream reader to read the file
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string line;
                        // Read the file line by line  
                        while ((line = sr.ReadLine()) != null)
                        {
                            // create a command object with each line and add to pcb
                            c = new Command(line);
                            pcb.Add(c);
                            count++;
                        }
                    }
                }
                return count;
            }
            else
            {
                return -1;
            }
        } 
        
        public List<Command> Pcb {
            get {return pcb;}
        }
        public List<PageTable> PageTable {
            get { return pageTable; }
            set { pageTable = value; }
        }
        public MFrame[] Memory {
            get { return memory; }
            set { memory = value; }
        }
        public List<int> FreeFrames
        {
            get { return freeFrames; }
            set { freeFrames = value; }
        }

        /****************************************************************
         * 
         * This method handles the next command in the pcb. It loads the 
         * process into a new page table, then it loads it into memory.
         * If it is a halt command, it removes the process from memory
         * and the page tablefrom the list of page tables.
         * 
         * Returns a -1 if it is at the end of the pcb otherwise it returns 
         * the number of commands left.
         * 
         ***************************************************************/
        public int Next()
        {
            if (pcb.Count < 1)
                return -1;
            // remove the next command and place into c
            Command c = pcb[0];
            pcb.RemoveAt(0);
            //previousPcb.Insert(0, pcb[0]);
            //previousFreeFrames.Insert(0, freeFrames);

            // check if it is a halt command or not
            if ( c.TextSize >= 0)
            {
                //previousPageTables.Insert(0, pageTable);
                //previousMemory.Insert(0, memory);

                // create a new page table for the new process
                PageTable pt = new PageTable(c);
                pt.SetPages();
                pageTable.Add(pt);

                // load the page into memory
                LoadIntoMemory(pageTable.Last());
            }
            else // it was a halt command
            {
                // loops through all memory frames and if it contains the process
                // that is being halted, remove it from memory
                for ( int x = 0; x <= 7; x++)
                {
                    if ( memory[x] != null && memory[x].ProcessID == c.ProcessID)
                    {
                        memory[x] = null;
                        // add that frame to the free frames list
                        freeFrames.Add(x);
                    }
                }
                // loops through page tables and remove the page table that is for the 
                // halting process
                for (int i = pageTable.Count - 1; i >= 0; i--)
                {
                    if ( pageTable[i].ProcessID == c.ProcessID)
                    {
                        pageTable.Remove(pageTable[i]);
                    }
                }

            }
            return pcb.Count;
        }

        //public int Previous()
        //{
        //    pcb.Insert(0, previousPcb[0]);
        //    previousPcb.RemoveAt(0);
        //    memory = previousMemory[0];
        //    previousMemory.RemoveAt(0);
        //    pageTable = previousPageTables[0];
        //    previousPageTables.RemoveAt(0);
        //    freeFrames = previousFreeFrames[0];
        //    previousFreeFrames.RemoveAt(0);
        //    return previousPcb.Count;
        //}
        
        /*****************************************************************
         *
         * This method takes a page table and loads it into memory.
         * 
         ****************************************************************/

        private void LoadIntoMemory(PageTable pt)
        {
            MFrame mf;
            // loops through page table and adds each page to the next available frame
            // in memory
            foreach( ProcessPage p in pt.Pages)
            {
                mf = new MFrame(p);

                // grab the next free frame 
                int freeFrame = freeFrames[0];
                freeFrames.RemoveAt(0);

                // set the page tabels frame value to the frame its being placed in
                p.FrameNumber = freeFrame;

                // set the frame value of the frame
                mf.FrameNumber = freeFrame;

                // place frame into memory in the free frame
                memory[freeFrame] = mf;
            }
        }

    }
    
    /*******************************************************************************
     * 
     * This is the command class. It stores commands as 3 int's. The process id, 
     * the size of the text segment and the size of the data segment.
     * 
     ******************************************************************************/

    class Command
    {
        private int processID;
        private int textSize;
        private int dataSize;

        /**************************************************************************
         * 
         * Constructer takes in a string as a command and parses it into the 3 values.
         * 
         * 
         **************************************************************************/
        public Command(string command)
        {
            string[] splitCommand = command.Split(' ');
            if( splitCommand.Length == 2)
            {
                processID = Int32.Parse(splitCommand[0]);

                // sets the size values to -1 if the command is a halt command
                textSize = -1;
                dataSize = -1;
            }
            else
            {
                processID = Int32.Parse(splitCommand[0]);
                textSize = Int32.Parse(splitCommand[1]);
                dataSize = Int32.Parse(splitCommand[2]);
            }
        }

        public int ProcessID {
            get { return processID; }
            set { processID = value; }
        }
        public int DataSize {
            get { return dataSize; }
            set { dataSize = value; }
        }
        public int TextSize {
            get { return textSize; }
            set { textSize = value; }
        }
    }


    /*********************************************************************
     * 
     * This is the Memory Frame class. It stores the memory frames information 
     * in 3 int's and a string. The frame number, the process id, the page 
     * number and the segment. 
     * 
     ********************************************************************/

    class MFrame
    {
        private int frameNumber;
        private int processID;
        private string segment;
        private int pageNumber;

        /*****************************************************************
         * 
         * Constructor takes in a page entry and copies the process id, segment
         * and page number over. It sets frame number to 0 by default.
         * 
         ****************************************************************/
        public MFrame(ProcessPage p)
        {
            this.frameNumber = 0;
            this.processID = p.ProcessID;
            this.segment = p.Segment;
            this.pageNumber = p.PageNumber;
        }

        public int FrameNumber {
            get { return frameNumber; }
            set { frameNumber = value; }
        }
        public string Segment {
            get { return segment; }
        }
        public int ProcessID {
            get { return processID; }
        }
        public int PageNumber {
            get { return pageNumber; }
        }
    }

    /**************************************************************************
     * 
     * This is the page table class. It sets up and stores a page table for a 
     * single process.
     * 
     *************************************************************************/

    class PageTable
    {
        private int processID;
        private int text;
        private int data;
        private List<ProcessPage> pages;

        private const int PAGE_SIZE = 512;

        /********************************************************************
         * 
         * Constructor takes in a command and initializes the class variables.
         * 
         *******************************************************************/
        public PageTable(Command command)
        {
            processID = command.ProcessID;
            text = command.TextSize;
            data = command.DataSize;
            pages = new List<ProcessPage>();
        }

        /*********************************************************************
         * 
         * This method loops through the text and data sizes, subtracting one 
         * page size each time until it reaches 0 bytes left and creates a table
         * with each one, making sure to keep track of the page number properly
         * by seperating the text segments from the data segments. 
         * 
         *********************************************************************/

        public void SetPages()
        {
            int tempText = text;
            int tempData = data;
            ProcessPage page;

            // loop through text segment
            while (tempText > 0)
            {
                // creates a new page
                page = new ProcessPage();
                tempText -= PAGE_SIZE;

                // if its not the first page, add one to the previous page number
                if (pages.Count > 0)
                {
                    int last = pages[pages.Count - 1].PageNumber;
                    page.PageNumber = last + 1;
                }

                page.ProcessID = processID;
                page.Segment = "Text";

                // add to list of pages
                pages.Add(page);
            }

            // loop through data segment
            while (tempData > 0)
            {
                // create new page
                page = new ProcessPage();
                tempData -= PAGE_SIZE;

                // if it is not the first data page then add one to the previous page number
                if (pages.Count > 0 && !pages.Last().Segment.Equals("Text"))
                {
                    int last = pages[pages.Count - 1].PageNumber;
                    page.PageNumber = last + 1;
                }

                page.ProcessID = processID;
                page.Segment = "Data";

                // add to list of pages
                pages.Add(page);
            }
        }

        public int ProcessID {
            get { return processID; }
        }
        public List<ProcessPage> Pages {
            get { return pages; }
        }
    }

    /*******************************************************************
     * 
     * This is a page class to store information about a page.
     * 
     ******************************************************************/

    class ProcessPage
    {
        private int processID;
        private int pageNumber;
        private string segment;
        private int frameNumber;

        /****************************************************************
         * 
         * Constructor sets segment to "" and page number and frame number to 0
         * 
         ***************************************************************/
        public ProcessPage()
        {
            pageNumber = 0;
            segment = "";
            frameNumber = 0;
        }

        public int FrameNumber
        {
            get { return frameNumber; }
            set { frameNumber = value; }
        }
        public string Segment
        {
            get { return segment; }
            set { segment = value; }
        }
        public int ProcessID
        {
            get { return processID; }
            set { processID = value; }
        }
        public int PageNumber
        {
            get { return pageNumber; }
            set { pageNumber = value; }
        }
    }
}
