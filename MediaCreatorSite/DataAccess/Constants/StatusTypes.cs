using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCreatorSite.DataAccess.Constants
{
    public static class StatusTypes
    {
        //There is a backend function that deletes all rows in all tables with this.
        public static StaticStatusType SET_TO_WIPE = new StaticStatusType() { Id = 0, Name = "set_to_wipe" };
        //This is if you have crud operations needed
        public static StaticStatusType DELETED = new StaticStatusType() { Id = 1, Name = "deleted"};
        public static StaticStatusType PROCESSING = new StaticStatusType() { Id = 2, Name = "processing"};
        public static StaticStatusType WAITING = new StaticStatusType() { Id = 3, Name = "waiting" };
        public static StaticStatusType FAILED = new StaticStatusType() { Id = 4, Name = "failed" };
        public static StaticStatusType COMPLETE = new StaticStatusType() { Id = 5, Name = "complete" };
        public static StaticStatusType USABLE = new StaticStatusType() { Id = 6, Name = "usable" };
        public static StaticStatusType READY = new StaticStatusType() { Id = 7, Name = "ready" };
        public static StaticStatusType COLLECTABLE = new StaticStatusType() { Id = 8, Name = "collectable" };
        public static StaticStatusType UPLOADING_TO_YOUTUBE = new StaticStatusType() { Id = 9, Name = "uploading to youtube" };
        public static StaticStatusType UPLOAD_TO_YOUTUBE = new StaticStatusType() { Id = 10, Name = "upload to youtube" };
        public static List<StaticStatusType> ALL_STATUS_TYPES = new List<StaticStatusType>()
        {
            SET_TO_WIPE, DELETED, PROCESSING, WAITING, FAILED, COMPLETE, USABLE, READY, COLLECTABLE, UPLOADING_TO_YOUTUBE, UPLOAD_TO_YOUTUBE
        };
    }

    public class StaticStatusType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
