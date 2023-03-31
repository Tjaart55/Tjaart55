Skip to content
Search or jump to…
Pull requests
Issues
Marketplace
Explore
 
@Tjaart55 
Tjaart55
/
C_Get_Closest_Coords
Public
Code
Issues
Pull requests
Actions
Projects
Wiki
Security
Insights
Settings
C_Get_Closest_Coords/GetClosestGPS.c
@Tjaart55
Tjaart55 Add files via upload
Latest commit 3a6bca5 2 hours ago
 History
 1 contributor
141 lines (113 sloc)  4.85 KB
   

#include <stdio.h>
#include <stdlib.h>
#include <time.h>

typedef struct Location_
{
    float lat;
    float lon;
}Location;

#pragma pack(push,1)
struct LocationInfo
{
    unsigned int PositionId;//4
    unsigned char VehicleReg[10];//10
    Location location;//8
    unsigned long long EpochDate;//8
}LocInfo, Linfo[2000000];
#pragma pack(pop)

struct AllInfo
{
   struct LocationInfo ResultLocations;
   Location ClosestLocTo;
   Location result;
   float ClosestDistance;

}AllCords[10];

int main()
{
    clock_t begin = clock();
    FILE* fp;
    fp = fopen("VehiclePositions.dat", "rb");

    if (fp) {

       
        fread(&Linfo, sizeof(struct LocationInfo),2000000, fp);


        AllCords[0].ClosestLocTo.lat = 34.544909;
        AllCords[0].ClosestLocTo.lon = -102.100843;
        AllCords[1].ClosestLocTo.lat = 32.345544;
        AllCords[1].ClosestLocTo.lon = -99.123124;
        AllCords[2].ClosestLocTo.lat = 33.234235;
        AllCords[2].ClosestLocTo.lon = -100.214124;
        AllCords[3].ClosestLocTo.lat = 35.195739;
        AllCords[3].ClosestLocTo.lon = -95.348899;
        AllCords[4].ClosestLocTo.lat = 31.895839;
        AllCords[4].ClosestLocTo.lon = -97.789573;
        AllCords[5].ClosestLocTo.lat = 32.895839;
        AllCords[5].ClosestLocTo.lon = -101.789573;
        AllCords[6].ClosestLocTo.lat = 34.115839;
        AllCords[6].ClosestLocTo.lon = -100.225732;
        AllCords[7].ClosestLocTo.lat = 32.335839;
        AllCords[7].ClosestLocTo.lon = -99.992232;
        AllCords[8].ClosestLocTo.lat = 33.535339;
        AllCords[8].ClosestLocTo.lon = -94.792232;
        AllCords[9].ClosestLocTo.lat = 32.234235;
        AllCords[9].ClosestLocTo.lon = -100.222222;

        for (int i = 0; i < 10; i++)
        {
            AllCords[i].ClosestDistance = LONG_MAX;
        }

       

        for (long i = 0; i <2000000; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                float x = AllCords[j].ClosestLocTo.lat;
                float y = AllCords[j].ClosestLocTo.lon;
                float dx = Linfo[i].location.lat - x;
                float dy = Linfo[i].location.lon - y;

                float distanceSquared = (dx * dx) + (dy * dy);

                if (distanceSquared < AllCords[j].ClosestDistance)
                {
                    AllCords[j].ClosestDistance = distanceSquared;
                    AllCords[j].ResultLocations.location.lat = Linfo[i].location.lat;
                    AllCords[j].ResultLocations.location.lon = Linfo[i].location.lon;
                    AllCords[j].ResultLocations.PositionId = Linfo[i].PositionId;
                    AllCords[j].ResultLocations.EpochDate = Linfo[i].EpochDate;
                    sprintf(AllCords[j].ResultLocations.VehicleReg, Linfo[i].VehicleReg);
                }
            }
        }

       //takes longer
      /*  while (fread(&LocInfo, sizeof(struct LocationInfo), 1, fp))
        {
   
            for (int j = 0; j < 10; j++)
            {
                float x = AllCords[j].ClosestLocTo.lat;
                float y = AllCords[j].ClosestLocTo.lon;
                float dx = LocInfo.location.lat - x;
                float dy = LocInfo.location.lon - y;
                float distanceSquared = (dx * dx) + (dy * dy);
               
                if (distanceSquared < AllCords[j].ClosestDistance)
                {
                    AllCords[j].ClosestDistance = distanceSquared;
                    AllCords[j].ResultLocations.location.lat = LocInfo.location.lat;
                    AllCords[j].ResultLocations.location.lon = LocInfo.location.lon;
                    AllCords[j].ResultLocations.PositionId = LocInfo.PositionId;
                    AllCords[j].ResultLocations.EpochDate = LocInfo.EpochDate;
                    sprintf(AllCords[j].ResultLocations.VehicleReg, LocInfo.VehicleReg);
                }
            }
           
        }*/

       fclose(fp);

       for (int i = 0; i < 10; i++)
       {
           char ttime[80];
           struct tm  ts;
           time_t tt = AllCords[i].ResultLocations.EpochDate;
           ts = *localtime(&tt);
           strftime(ttime, sizeof(ttime), "%a %Y-%m-%d %H:%M:%S", &ts);// Format time, "ddd yyyy-mm-dd hh:mm:ss zzz"
           printf("The closest truck[%s] with id:%u to position %d[%f,%f] was at location: %f,%f on %s\n", AllCords[i].ResultLocations.VehicleReg, AllCords[i].ResultLocations.PositionId,i+1, AllCords[i].ClosestLocTo.lat, AllCords[i].ClosestLocTo.lon, AllCords[i].ResultLocations.location.lat, AllCords[i].ResultLocations.location.lon, ttime);
       }
    
    }
    //_sleep(1000);
    clock_t end = clock();
    double time_spent = (double)(end - begin) / CLOCKS_PER_SEC;
    printf("The execution time of the program was %lf seconds", time_spent);

    getchar();
}


© 2022 GitHub, Inc.
Terms
Privacy
Security
Status
Docs
Contact GitHub
Pricing
API
Training
Blog
About
Loading complete