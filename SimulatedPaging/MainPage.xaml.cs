/***************************************************************
    * 
    * Nathan Kelderman
    * Professor Wolffe
    * CIS 452 - Simulated Paging GUI
    * 4/5/2018
    * 
    ***************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at 
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimulatedPaging
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // create a new instance of Backend for use throughout class
        private Backend b;
        public MainPage()
        {
            this.InitializeComponent();
            // initialize Backend
            b = new Backend();
        }

        /*******************************************************************
         * 
         * This function handles the enter button being clicked.
         * 
         *******************************************************************/

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Content.ToString();
            // check to see if the button was Enter
            if (content.Equals("Enter"))
            {
                // take in the entered filename and send to Backend to read into pcb
                string filename = Filename.Text;
                int x = b.ReadFile(filename);

                // if read was successful, print the trace tape to a text box
                if (x > 0)
                {
                    List<Command> commands = new List<Command>(b.Pcb);
                    String pcb = "";
                    foreach (Command c in commands)
                    {
                        if (c.TextSize > 0)
                        {
                            pcb += c.ProcessID + " " + c.TextSize + " " + c.DataSize + "\n";
                        }
                        else
                        {
                            pcb += c.ProcessID + " Halt" + "\n";
                        }
                    }
                    
                    PCB.Text = pcb;
                    Executing.Text = "Executing";

                    // enable the next button and disable the enter button to 
                    // control user input
                    Next.IsEnabled = true;
                    Previous.IsEnabled = false;
                    Enter.IsEnabled = false;
                }
                else
                {
                    PCB.Text = "File could not be found.\nPlease try again.";
                }
            }
        }

        /************************************************************************
         * 
         * This function handles the user clicking the next button.
         * 
         ***********************************************************************/
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            // tell the Backend to execute the next command
            int x = b.Next();
            //Previous.IsEnabled = true;
            // if this is the last command, disable the next button and 
            // enable the enter button again so the user can enter a new file
            if (x == 0)
            {
                Next.IsEnabled = false;
                Executing.Text = "FINISHED EXECUTING \nTRACE TAPE\n\nEnter " +
                    "another file if you wish to run again";
                Enter.IsEnabled = true;
            }

            // updates the display
            Update_Display();
        }
        
        /****************************************************************************
         * 
         * This method goes through and updates the graphical page table list and the 
         * graphical memory frames. 
         * 
         ****************************************************************************/
        
        public void Update_Display()
        {
            // clears the old page table and grabs the new one from backend to display
            PageTable.Items.Clear();
            foreach ( PageTable pt in b.PageTable)
            {
                PageTable.Items.Add("Process: " + pt.ProcessID);
                PageTable.Items.Add("Page  Segment  Frame");
                foreach ( ProcessPage p in pt.Pages)
                {
                    PageTable.Items.Add(p.PageNumber + "\t" + p.Segment + "\t" + 
                                            p.FrameNumber);
                }
            }

            // goes through each frame from Backends frame list and it it is occupied, 
            // it prints the appropriate information
            if (b.Memory[0] == null)
                Frame0.Text = "";
            else
                Frame0.Text = "      " + b.Memory[0].ProcessID + "\t          " + 
                    b.Memory[0].Segment + "\t" + b.Memory[0].PageNumber;

            if (b.Memory[1] == null)
                Frame1.Text = "";
            else
                Frame1.Text = "      " + b.Memory[1].ProcessID + "\t          " + 
                    b.Memory[1].Segment + "\t" + b.Memory[1].PageNumber;

            if (b.Memory[2] == null)
                Frame2.Text = "";
            else
                Frame2.Text = "      " + b.Memory[2].ProcessID + "\t          " + 
                    b.Memory[2].Segment + "\t" + b.Memory[2].PageNumber;

            if (b.Memory[3] == null)
                Frame3.Text = "";
            else
                Frame3.Text = "      " + b.Memory[3].ProcessID + "\t          " + 
                    b.Memory[3].Segment + "\t" + b.Memory[3].PageNumber;

            if (b.Memory[4] == null)
                Frame4.Text = "";
            else
                Frame4.Text = "      " + b.Memory[4].ProcessID + "\t          " + 
                    b.Memory[4].Segment + "\t" + b.Memory[4].PageNumber;

            if (b.Memory[5] == null)
                Frame5.Text = "";
            else
                Frame5.Text = "      " + b.Memory[5].ProcessID + "\t          " + 
                    b.Memory[5].Segment + "\t" + b.Memory[5].PageNumber;

            if (b.Memory[6] == null)
                Frame6.Text = "";
            else
                Frame6.Text = "      " + b.Memory[6].ProcessID + "\t          " + 
                    b.Memory[6].Segment + "\t" + b.Memory[6].PageNumber;

            if (b.Memory[7] == null)
                Frame7.Text = "";
            else
                Frame7.Text = "      " + b.Memory[7].ProcessID + "\t          " + 
                    b.Memory[7].Segment + "\t" + b.Memory[7].PageNumber;

            Executing.Text = "\n" + Executing.Text;
        }

        // attempted but non-functionaing previous buttom
        //private void Previous_Click(object sender, RoutedEventArgs e)
        //{
        //    if ( b.Previous() == 0)
        //    {
        //        Previous.IsEnabled = false;
        //    }
        //    Update_Display();
        //}
    }
}
