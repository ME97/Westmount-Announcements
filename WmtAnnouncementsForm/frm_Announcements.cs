/* Name(s): Miles Eastwood (In collaboration with Omar and Mr. Trink)
 * Date: May-June 2014
 * Project: Westmount Announcements Digitilzation
 * 
 * Details: This program acesses three seperate xml feeds; one for announcements from a hwdsb.commons page, one for events from a
 *  google calendar, and one from environment canada hamilton for the weather. Press 'f' to toggle fullscreen. Press 'q' to close program.
 * 
 * Note: Keep announcement titles fairly short, about 30 characters max, including spaces and puncuation(For example:"Westmount beats Westdale"
 *          If a title is too long it won't crash the program or overlap anything, but simply be cut off
 * 
 * Events are currently being parsed from an xml feed of the westmountannouncements@gmail.com google calendar
 * Need to make sure that that feed can be accessed even when the gmail account isnt logged in (make calendar public?)
 *              
 * wmtevents.commons.hwdsb.on.ca
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.ServiceModel.Web;
using System.ServiceModel.Syndication;
using System.Net;
using System.Text.RegularExpressions;

namespace WmtAnnouncementsForm
{
    public partial class frm_Announcements : Form
    {
        //ALL OF THE LISTS/VARIABLES/OBJECTS THAT ARE USED IN THE PROGRAM

        //A list containing each of the announcements, made with their own class
        List<Announcement> announcementList = new List<Announcement>();

        //A list containing each of the events, made with their own class and taken from a google calendar
        List<Event> eventList = new List<Event>();

        //List used to store weather data for each day in the form of a weather class object
        List<WeatherClass> weatherClassList = new List<WeatherClass>();
        //This list contains the correct indexes for taking from the environment canada xml feed (1 is current, 4 is tommorrow, 5 is day after tomorrow, 6 is next day
        List<int> correctValues = new List<int>() { 1, 4, 5, 6 };

        //The actual string that scrolls across the screen; a concatanation of all of the event classes
        string eventStream = "";

        //A list containing each of the images
        List<Image> slideshowList = new List<Image>();
        //A count used to iterate through the slides in the slideshow
        int slideshowCount = 0;

        //Variables used in the FormatImage function, which adjusts the width and height of images in the slideshow, making sure the width:height ratio remains the same
        double imageRatio = 0;
        int imageWidth = 0;
        int imageHeight = 0;

        //Variable used to compare dates
        int nowDay = 0;

        //Ratios used to adjust things such as font size, spacing
        double width = 0;
        double height = 0;
        double widthHeightRatio = 0;

        //A count that increases every thirty seconds, which changes the announcment being shown
        int thirtySecondCount = 0;

        //Used for swapping between fullscreen and minimized, just for testing
        bool fullScreen = true;
        bool fullScreenInitializer = false;

        //These variables turn true on the first time each element is initialized.
        //If there is a connection fail and these are false (ie the program just started), precooked elements are used
        //If there is a connection fail and these are true (ie the program connected when started but failed on an update), the elements lists remain unchanged
        bool announcementsInitialized = false;
        bool eventsInitialized = false;
        bool weatherInitialized = false;

        public frm_Announcements() 
        {
            //Initializes the form, running all of the Initialize functions

            InitializeComponent();
            UpdateAnnouncements();
            UpdateEvents();
            UpdateWeather();
            UpdateCurrentDate();
            UpdateSlideshow();
        }

        void UpdateAnnouncements()
        {
            //Checks for server connection, and updates the the announcements list with the new objects
            if (CheckForServerConnection("http://wmtnews.commons.hwdsb.on.ca/feed/"))
            {
                announcementList.Clear();
                announcementsInitialized = true;
                var reader = XmlReader.Create("http://wmtnews.commons.hwdsb.on.ca/feed/");
                var feed = SyndicationFeed.Load<SyndicationFeed>(reader);
                
                foreach (var item in feed.Items)
                {
                    Announcement announcement = new Announcement(item.Title.Text, item.Summary.Text, item.PublishDate.ToString());
                    announcementList.Add(announcement);
                }
            }
            else if (announcementsInitialized == false)
            {
                announcementsInitialized = true;
                announcementList.Add(new Announcement("Announcements Currently Offline", "Sorry for any Inconveience", DateTime.Today.ToString()));
            }
        }

        void UpdateEvents()
        {
            //If the internet isnt working, the event stream doesnt update, but the program doesnt crash
            if (CheckForServerConnection("https://www.google.com/calendar/feeds/westmountannouncements%40gmail.com/private-401ea51840c7edbadaa2c0cee7f323ef/basic"))
            {
                eventList.Clear();
                eventStream = "";
                eventsInitialized = true;
                //Accesses the google calender (account westmountannouncements@gmail.com)
                var reader = XmlReader.Create("https://www.google.com/calendar/feeds/westmountannouncements%40gmail.com/private-401ea51840c7edbadaa2c0cee7f323ef/basic");
                var feed = SyndicationFeed.Load<SyndicationFeed>(reader);

                foreach (var item in feed.Items)
                {
                    //Only shows events that are between today and two weeks from today
                        if (DateTime.Compare(Convert.ToDateTime(item.Summary.Text.Substring(6, 10)), DateTime.Now) >= 0 && DateTime.Compare(Convert.ToDateTime(item.Summary.Text.Substring(6, 10)), DateTime.Now.AddDays(14)) <= 0)
                        {
                            eventList.Add(new Event(item.Title.Text, Convert.ToDateTime(item.Summary.Text.Substring(6, 10))));
                        }
                    
                }

                //Each of the events is added to one long string which is then scrolled through
                foreach (var item in eventList)
                {
                    eventStream += item.getDisplayString();
                }
                reader.Close();
            }
            else if (eventsInitialized == false)
            {
                eventsInitialized = true;
                eventStream = "        Current Events Not Available         ";
            }
        }

        void UpdateWeather()
        {
            //Connects to Environment Canada Hamilton page and takes the appropriate information
            string url = @"http://weather.gc.ca/rss/city/on-77_e.xml";
            if (CheckForServerConnection("http://weather.gc.ca/rss/city/on-77_e.xml"))
            {
                weatherClassList.Clear();
                weatherInitialized = true;
                XmlReader reader = XmlReader.Create(url);
                SyndicationFeed weatherFeed = SyndicationFeed.Load(reader);
                reader.Close();

                //These variables are used to control which items get added to the weatherClassList
                int count = 0;
                int dayCount = 0;
                foreach (SyndicationItem item in weatherFeed.Items)
                {
                    //The indecies it looks for are based off of how the EnviroCanada xml feed is formatted (1 is current, 4 is tomorrow, 5 is day after tomorrow, 6 is day after that)
                    if (correctValues.Contains(count))
                    {
                        weatherClassList.Add(new WeatherClass(item.Title.Text, DateTime.Now.AddDays(dayCount).DayOfWeek.ToString(), count));
                        dayCount++;
                    }
                    count++;
                }
            }
            else if(weatherInitialized == false)
            {
                weatherInitialized = true;
                foreach (var item in correctValues)
                {
                    weatherClassList.Add(new WeatherClass("noData", "", 10));
                }
            }
        }

        private void tmr_UpdateElements_Tick(object sender, EventArgs e)
        {
            //Updater for everything. Currently ticks every 30 minutes

            //Temporarily disables timers as to not throw index-less-than-zero exceptions
            tmr_PaintRefresh.Enabled = false;
            tmr_thirtySeconds.Enabled = false;

            UpdateAnnouncements();
            UpdateEvents();
            UpdateWeather();
            UpdateCurrentDate();

            tmr_PaintRefresh.Enabled = true;
            tmr_thirtySeconds.Enabled = true;
        }
    
        public bool CheckForServerConnection(string url)
        {
            //This functions takes a url and checks to see if there is a connection
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead(url))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        void UpdateCurrentDate()
        {
            //This function is used control which announcements are shown (only today or yesterday, this can be changed in paint function)

            string today = DateTime.Today.ToString();
            nowDay = int.Parse(today.Substring(8, 2));
            //nowDay = 29;
        }

        public void ImageFormat()
        {
            //This function takes an image, compares the width and height, taking the larger of the two as well as the ratio, and adjusting each to fit based on the resolution

            Image currentImage = slideshowList[slideshowCount];

            if (currentImage.Height >= currentImage.Width)
            {
                imageRatio = currentImage.Height / (float)currentImage.Width;
                imageHeight = (int)(height * 0.43);
                imageWidth = (int)(imageHeight / imageRatio);
            }
            else if (currentImage.Width >= currentImage.Height)
            {
                imageRatio = currentImage.Width / (float)currentImage.Height;
                imageWidth = (int)(height * 0.43);
                imageHeight = (int)(imageWidth / imageRatio);
            }
        }

        private void frm_Announcements_Paint(object sender, PaintEventArgs e)
        {
            //The paint function, the largest of all of the functions. Paints all rectangles, textboxes, images, and strings
            //Updates 30 times a second

            //Obligitory assignment of easy to use graphics object
            Graphics g = e.Graphics;

            //Colour as sampled from westmount logo seen in top left of this form
            Color WestmountBlue = new Color();
            WestmountBlue = Color.FromArgb(101, 172, 216);
            this.BackColor = WestmountBlue;

            //Variables used to position everything that is drawn on the screen, and that adjusts based on resolution
            widthHeightRatio = this.Width / (double)this.Height;
            height = (double)this.Height;
            width = height * widthHeightRatio;

            //This is the header of the whole form, which is always drawn
            Font headerFont = new Font("Arial", (int)(width * 0.04));
            g.DrawString("Westmount Announcements", headerFont, Brushes.Black, (int)(0.15 * width), (int)(0.01 * height));

            //Clock that updates every tick, showing current date and time
            Font clockFont = new Font("Arial", (int)((int)(height * 0.08) / 2));
            g.DrawString(DateTime.Now.ToString(), clockFont, Brushes.Black, (int)(0.3 * width), (int)(0.125 * height));

            //Weather information and images drawn to screen

            //Rectangle used to surround where the weather goes
            Rectangle weatherBox = new Rectangle((int)(width * 0.61), (int)(height * 0.2), (int)(width * 0.36), (int)(height * 0.2));
            g.DrawRectangle(Pens.Black, weatherBox);

            //This is for the current weather. The first g. gets and draws the icon. The next displays the current conditions (e.g Sunny), the third shows the current temperature
            Font currentWeatherFont = new Font("Arial", (int)(width * 0.02));
            g.DrawImage(weatherClassList[0].getConditionImage(), (int)(width * 0.61), (int)(height * 0.2), (int)(height * 0.15), (int)(height * 0.15));
            g.DrawString("Now: " + weatherClassList[0].getCurrentConditions(), currentWeatherFont, Brushes.Black, (int)(width * 0.71), (int)(height * 0.21));
            g.DrawString(weatherClassList[0].getHigh(), currentWeatherFont, Brushes.Black, (int)(width * 0.61), (int)(height * 0.35));

            //These three pairs of method calls draw the forcast for the next three days, showing the weather icon (e.g a cloud), the day of the week (e.g sunday), and the expected high
            Font futureWeatherFont = new Font("Arial", (int)(height * 0.02));
            //Tomorrows forcast
            g.DrawImage(weatherClassList[1].getConditionImage(), (int)(width * 0.705), (int)(height * 0.3), (int)(height * 0.09), (int)(height * 0.09));
            g.DrawString(weatherClassList[1].getDOW() + ":" + weatherClassList[1].getHigh(), futureWeatherFont, Brushes.Black, (int)(width * 0.715), (int)(height * 0.275));

            //Day after tomorrows forcast
            g.DrawImage(weatherClassList[2].getConditionImage(), (int)(width * 0.8), (int)(height * 0.3), (int)(height * 0.09), (int)(height * 0.09));
            g.DrawString(weatherClassList[2].getDOW() + ":" + weatherClassList[2].getHigh(), futureWeatherFont, Brushes.Black, (int)(width * 0.8), (int)(height * 0.275));

            //Forcast for 3 days from now
            g.DrawImage(weatherClassList[3].getConditionImage(), (int)(width * 0.9), (int)(height * 0.3), (int)(height * 0.09), (int)(height * 0.09));
            g.DrawString(weatherClassList[3].getDOW() + ":" + weatherClassList[3].getHigh(), futureWeatherFont, Brushes.Black, (int)(width * 0.9), (int)(height * 0.275));

            //Westmount Logo in top left corner
            g.DrawImage(WmtAnnouncementsForm.Resource1.westmountLogo, 0, 0, (int)(height * 0.2), (int)(height * 0.2));

            //Picture slideshow, which currently just takes for a folder on my harddrive. Therefore, this would crash the program if ran on another computer
            ImageFormat();
            g.DrawImage(slideshowList[slideshowCount], (int)(width * 0.61), (int)(height * 0.42), imageWidth, imageHeight);

            //A black rectangle drawn around the announcements
            Rectangle announcementBox = new Rectangle(0, (int)(height * 0.2), (int)(width * 0.6), (int)(height * 0.65));
            g.DrawRectangle(Pens.Black, announcementBox);


            widthHeightRatio = this.Width / (double)this.Height;
            height = (double)this.Height;
            width = height * widthHeightRatio;


            //The text box for the body of each announcement
            this.bodyTextBox.Left = (int)(width * 0.1);
            this.bodyTextBox.Top = (int)(height * 0.6);
            this.bodyTextBox.Width = (int)(width * 0.58);
            this.bodyTextBox.Height = (int)(height * 0.65);
            this.bodyTextBox.BackColor = Color.Black;
            this.bodyTextBox.Font = new Font("Arial", (int)Math.Ceiling(height * 0.03));

            //The text box for the title of each announcement
            this.titleTextBox.Left = (int)(width * 0.01);
            this.titleTextBox.Top = (int)(height * 0.23);
            this.titleTextBox.Width = (int)(width * 0.58);
            this.titleTextBox.Height = (int)(height * 0.09);
            this.titleTextBox.BackColor = WestmountBlue;
            this.titleTextBox.Font = new Font("Arial", (int)(width * 0.025), FontStyle.Bold);



            //Scrolling string that shows the events, as taken from the eventupdater
            g.DrawString(eventStream, headerFont, Brushes.Black, 0, (int)(height * 0.863));
            eventStream = eventStream.Substring(1, eventStream.Length - 1) + eventStream.Substring(0, 1);

            //This if statement puts the program into fullscreen upon starting. Note that this can be toggled by pressing the 'f' key
            //Also formats and sets the announcements, so that you don't have to wait for the 30 seconds for the timer to tick
            if (fullScreenInitializer == false)
            {
                fullScreenInitializer = true;
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.Bounds = Screen.PrimaryScreen.Bounds;
                PaintAnnouncements();
            }
        }

        private void tmr_PaintRefresh_Tick(object sender, EventArgs e)
        {
            //Ticks 30 times a seconds to change the formatting if resolution changes
            Refresh();
        }

        private void PaintAnnouncements()
        {

            //Only displays announcements that were posted today or yesterday
            if (nowDay == announcementList[thirtySecondCount].getDay() || nowDay - 1 == announcementList[thirtySecondCount].getDay())
            {
                this.bodyTextBox.Text = announcementList[thirtySecondCount].getBody();

                this.titleTextBox.Text = announcementList[thirtySecondCount].getTitle();
            }
        }

        private void tmr_thirtySeconds_Tick(object sender, EventArgs e)
        {
            //This timer ticks every thirty seconds, used to cycle through the announcements
            thirtySecondCount++;
            if (thirtySecondCount > announcementList.Count() - 1)
            {
                thirtySecondCount = 0;
            }
            PaintAnnouncements();
        }

        /*The following three classes are used as a medium for holding information that 
         * needs to be taken from an online source and displayed on the screen.
         * The use of classes allows for lists to be easily cycled through, 
         * and also allows for multiple properties to be saved for each individual item.
         */
        class Announcement
        {
            //A very simply class, which simply takes the title and body in the form of strings which can then be added to textboxes
            //Publish date is also saved, and used for determining if the announcement should be shown, or if it is too old
            string title = "";
            string body = "";
            int postDay = 0;

            public Announcement(string curTitle, string curBody, string postDate)
            {
                //The htmldecode method takes care of any Numeric Character References, as the xml reader does not parse these into unicode on its own
                body = System.Net.WebUtility.HtmlDecode(curBody);
                title = System.Net.WebUtility.HtmlDecode(curTitle);
                postDay = int.Parse(postDate.Substring(8, 2));
            }

            public string getTitle()
            {
                return title;
            }

            public string getBody()
            {
                return body;
            }

            public int getDay()
            {
                return postDay;
            }
        }

        class Event
        {
            //This class stores an event from the google calendar, which is then added to the event scroller string
            string eventName = "";
            DateTime eventDate = new DateTime();
            string eventDisplay = "";

            public Event(string curEvent, DateTime curEventDate)
            {
                eventName = curEvent;
                eventDate = curEventDate;
            }

            public string getEventName()
            {
                return eventName;
            }

            public DateTime getEventDateTime()
            {
                return eventDate;
            }

            //The 'DisplayString' is what is actually concatenated to the event scroller. Spaces are added so that there are spaces between events
            public string getDisplayString()
            {
                //A list of all of the months, with index 0 set to an empty string
                List<string> monthList = new List<string>() {"", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};
                
                //The substring taken from the datetime will be a number like '01' for January. This is simply used as an index in order to find the correct month word.
                eventDisplay = eventName + ": " + monthList[int.Parse(eventDate.Month.ToString())] + " " + eventDate.ToString().Substring(8, 2) + "      ";
                return eventDisplay;
            }
        }

        class WeatherClass
        {
            //The most complicated of the three classes, due partly by the fact that there is more information to store, and partly by the fact that the xml from environment is not formatted as nicely

            //The day of the week is the actual word (eg sunday). This is only displayed for forcasted weather
            string dayOfWeek = "";
            //The expected daily high
            string expectedHigh = "";
            //The current conditions (e.g 'sunny'). Only used for current weather (obviously)
            string currentConditions = "";
            //The image that corresponds to the current or predicted weather conditions
            Image conditionImage;

            public WeatherClass(string xmlTitle, string curDOW, int count)
            {
                
                //This if-else is necessary due to the fact that the current conditions are formatted slightly differently than the next day's forcast (note the indexing of the comma rather than the period)
                if (count == 1)
                {
                    dayOfWeek = curDOW.Substring(0, 3);
                    conditionImage = getWeatherImage(xmlTitle);
                    expectedHigh = xmlTitle.Substring(xmlTitle.IndexOf(",") + 2, 5);
                    currentConditions = xmlTitle.Substring(xmlTitle.IndexOf(": ") + 2, xmlTitle.IndexOf(",") - (xmlTitle.IndexOf(": ") + 2));   
                }
                //If there is no internet connection, each weather element is assigned these variables. This way the program does not crash.
                else if (count == 10)
                {
                    dayOfWeek = "";
                    conditionImage = Properties.Resources.sunny;
                    expectedHigh = "";
                    currentConditions = "";
                }
                //This is the code run for the three future days (e.g NOT the current day and NOT the noData condition)
                else
                {
                    dayOfWeek = curDOW.Substring(0, 3);
                    conditionImage = getWeatherImage(xmlTitle);
                    /*//This if-else is needed due to the fact that the formatting for each element title in the 
                     * environment canada xml changes depending on the time of day.
                     * A bug was found where the 'expectedHigh' string would display 3 characters, instead of numbers.
                     * This is temporarily (maybe permanantly) resolved by trying to parse the result into an int.
                     * If the try parse is not succesful, expectedHigh becomes a blank string.
                     * The thing about this is that the formatting only changes during the night, so it should be a relative non-issue*/
                    int tempInt;
                    if (int.TryParse(xmlTitle.Substring(xmlTitle.IndexOf("High") + 4, 3), out tempInt))
                        expectedHigh = xmlTitle.Substring(xmlTitle.IndexOf("High") + 4, 3);
                    else
                        expectedHigh = "20";
                    currentConditions = xmlTitle.Substring(xmlTitle.IndexOf(": ") + 2, xmlTitle.IndexOf(".") - (xmlTitle.IndexOf(": ") + 2));
                }
            }

            public string getDOW()
            {
                return dayOfWeek;
            }

            public string getCurrentConditions()
            {
                return currentConditions;
            }

            public string getHigh()
            {
                return expectedHigh;
            }

            public Image getConditionImage()
            {
                return conditionImage;
            }

            //This function takes the title of an xml item, searches it for keywords from the dictionary, and assigns the apropriate resources file for the image
            //If none of the keywords match, a default 'noData' resource is shown
            Image getWeatherImage(string weatherCondition)
            {
                foreach (var key in weatherPics.Keys)
                {
                    if (weatherCondition.ToLower().Contains(key))
                    {
                        return weatherPics[key];
                    }
                }
                return Properties.Resources.noData;
            }
            //A dictionary containing the key-words as the keys, and the corresponding resource images as the values
            Dictionary<string, Image> weatherPics = new Dictionary<string, Image>() {{"sunny",Properties.Resources.sunny}, {"rain", Properties.Resources.rain2},
        {"partly cloudy", Properties.Resources.partly_cloudy},{"a few clouds", Properties.Resources.partly_cloudy},{"a mix of sun and cloud", Properties.Resources.partly_cloudy}, 
        {"mixed rain and snow", Properties.Resources.snow},{"sleet",Properties.Resources.snow}, {"cloudy", Properties.Resources.cloudy},
        {"mostly cloudy",Properties.Resources.cloudy},{"snow", Properties.Resources.snow},{"wind and rain", Properties.Resources.rain},
        {"thunder", Properties.Resources.thunder},{"shower", Properties.Resources.rain2}, 
        {"drizzle", Properties.Resources.rain2},{"fog", Properties.Resources.fog}, {"hail", Properties.Resources.freezing_rain}, 
        {"pellets", Properties.Resources.freezing_rain}, {"freezing",Properties.Resources.freezing_rain}, {"cold", Properties.Resources.cold}, 
        {"hot", Properties.Resources.hot}, {"heat", Properties.Resources.hot}, {"clear", Properties.Resources.fog}};

        }

        public void UpdateSlideshow()
        {
            //This function is the outline for taking picture files from an external folder (such as a school wide folder for teachers)
            //For the time being, it just takes from a local folder
            try
            {
                slideshowList.Clear();

                String[] filenames = System.IO.Directory.GetFiles("C:/Users/Miles/Pictures/slideshow");

                foreach (string filename in filenames)
                {
                    //Only takes picture files
                    if (filename.Contains(".jpg") || filename.Contains(".png") || filename.Contains(".jpeg") || filename.Contains(".gif"))
                    {
                        slideshowList.Add(Image.FromFile(filename));
                    }
                }
            }
            catch
            {
                slideshowList.Add(Resource1.westmountLogo);
            }
        }

        private void tmr_slideshow_Tick(object sender, EventArgs e)
        {
            //This tick cycles through the slideshow list, currently ticks every 5 seconds
            slideshowCount++;
            if (slideshowCount > slideshowList.Count() - 1)
            {
                slideshowCount = 0;
            }
        }

        private void frm_Announcements_KeyDown(object sender, KeyEventArgs e)
        {
            //Puts the form into fullscreen and back
            if (e.KeyCode == Keys.F)
                if (fullScreen == false)
                {
                    fullScreen = true;
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    this.Bounds = Screen.PrimaryScreen.Bounds;
                }
                else
                {
                    fullScreen = false;
                    this.Width = 800;
                    this.Height = 600;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                }
            //Closes the form upon 'Q' keypress
            if (e.KeyCode == Keys.Q)
            {
                Application.Exit();
            }
        }

        private void tmr_eventscroller_Tick(object sender, EventArgs e)
        {
            //Timer that causes the string to scroll
            eventStream = eventStream.Substring(1, eventStream.Length - 1) + eventStream.Substring(0, 1);
        }
    }
}
