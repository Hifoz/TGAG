using System;
/// <summary>
/// This class constains settigns related to threading and chnk launching
/// </summary>
public static class Settings {    

    public static int WorldGenThreads {
        get {
            int threads = Environment.ProcessorCount;
            switch (threads) {
                case 1: //Do single cores still exist?
                case 2: //non HT dual core
                    return 1;
                case 4: //non HT quad core or HT dual core
                    return 3;
                case 6:
                    return 4;
                case 8:
                    return 6;
                default:
                    if (threads >= 10) { //5 HT cores, or 10 real Cores and up (Never seen a 5 core though, but W/E)
                        return 8;        //Going beyond 8 threads has not shown much benefit for this game.
                    } else { //Some weird 3 core thingy maybe
                        return 2;
                    }
            }
        }
    }
}
