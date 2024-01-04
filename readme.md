# C# TimeSeries Library Handbook

## Introduction

TimeSeries is a set of tools to create, manage, clean and compute time series.
Data are located in a **List** of *Observation* so that tie time and measure so that they are never separated. The value is called *Measure* and is in double floating, the timestamp is called *Chron* and is a **DateTimeOffset** in C#.
It is therefore not genuinely a collection of arrays.

## 1. Class *Observation*

The core part of a time series is a **List** of *Observation* called *Observations* (plural). An Observation is a **Class**:

<code>
    public class Observation{   
            &emsp;&emsp;public long Idx { get; set; }
            &emsp;&emsp;public double Meas{ get; set; } // for Measure
            &emsp;&emsp;public DateTimeOffset Chron { get; set; } //The time is implicitely converted into UTC time
            &emsp;&emsp;public bool IsInvalid { get; set; }
            &emsp;&emsp;public string Status { get; set; }
            &emsp;&emsp;public double Dchron { get; set; }
            &emsp;&emsp;public double Dmeas { get; set; }
        }
</code>

A few comments:

- *Observation* is atomic. Although all the fields are public for simplicity, once an Observation is created the Measure, Chron and IsInvalid are not supposed to be modified. For testing purposes the fields are not limited; the setter should be deleted in productions.
- Dchron and Dmeas are to be completed later in the process.
- This is the Class that is processed and exported; not necessarily the one that is imported. It is as simple as possible.
- The *Meas* field is for measure. It is a IEE754 compatible floating point
- The *Status* is a string for controlling and is not mandatory.

### 1.1. Time - the *Chron* member field

The manipulation of time has always been a tricky topic. Here is a very short summary of how the time is used here

#### 1.1.1 What is time ?

If we simplify things there are two ways of computing time:

- Solar/Social time: It is the time we use and we understand. The solar year was considered the most precise until atomic clocks. It is well known that months and years are not of constant length; it is a bit less known that days are not constant any more since introduction of the leap second (correction of one or two seconds every six months on the length of the last day - this correction is not foreseeable). If a system drift away too much from the solar time, we will find ourselves without our usual landmarks.

- Atomic/UNIX time: It is the number of seconds that have elapsed since 00:00:00 UTC on 1 January 1970, the Unix ***Epoch***, without adjustments of leap seconds.

***Remarks***

-- 00:00:00 is the beginning of the day. It seems intuitive but it does not exist in US English. In the weird US date-time notation, it is noted as '12:00 AM', one minute after '11:59 PM'. Midday is therefore noted '12:00 PM', one minute after '11:59 AM' and one minute before '12:01 PM'. After one hour our '13:00' is '01:00 PM'

-- The Unix time does not have leap second and is therefore not aligned on the usual calendar. There is a collection of time clocks based on the constant passing of time (i.e. Atomic time, GPS time ...)

-- We do not need in this library to know the **exact** current time. Date and Time are a given in the Observation data.

-- UTC means "Univeral Time Coordinated". The inversion of adjective is an heritage of French. It is the only accepted word in ISO8601 definition of a date. It is also called "ZULU" time. "GMT" is a time zone that is coincidentaly the same as "UTC" but has only local meaning (UK).

-- Any time on the planet can be converted into UTC once we know the difference in time between UTC and Local Time. It is most of the time expressed in hours ("+02:00", "-07:00") but the ISO8601 leaves the possibility to have minutes gap with the UTC time. Some capitals use a 30 minute gap and even a 45 minutes gap.

-- The winter/summer time modify the time zone number attributed to a date.

-- As examples, Luxembourg is in UTC+1 during winter, UTC+2 during summer. As some of writers of conventions are pure idiots, the time in Luxembourg is in "Central European Time" during winter and "Central European Summer Time" in summer. I guess we can deduce that the standard time was the winter time. London is at UTC (what they call GMT) during winter but at UTC+1 during summer. Reykjavik, Accra, Bamako,Dakar,Abidjan,Monrovia, Freetown and Sao Tome are the only capitals working on UTC all year long. But this could change if daylight saving times are abandoned. Katmandu has a 05:45 gap with UTC.

-- The focus on time zone is important if we have to detect wether an event is produced before or after another one. It is therefore necessary to use time zone from the beginning and not playing with "Local Time" The closest countries to pose problems are UK on one side and Budapest on the other side.

-- The most efficient way to deal with date-time is to use Unix-like time computation and convert into "readable" format where needed. The Unix-like time is indeed not easy to read: the Unix-time of some time the Friday 22 december 2023 is 1703232078. It is not possible to use this format in social applications where you need to distinguish days, hours, months, etc.

#### 1.1.2. Time in IT

In the preceding example, the official time is therefore 1703232078, that is 1,703,232,078 seconds. This is not a IT definition. What when we need to deal with it in IT ?

- One obvious possibility is to use int32, which has a range of approximately +/- 2 10e9. This allows us to go only until January 19, 2038, which might be considered as short. On the other side negative allows us to go back in the past (December 13, 1901), but that may not be useful.

- Using unsigned int32 allows to go until 2106 but not before Epoch.

- int64 is a small step for a developer but a huge one in terms of range: +/- 9 10e18 is largely enough for any date until 9999.

Problem solved ? We actually have not mentioned fractions of seconds... C family languages are designed for granularity to the second. Any use of fraction of seconds was an addendum, and there is still consequences of this in the way date-time are processed in these languages.

One possibility almost intuitive is to use floating numbers. This solution has two cons:

-- There is no guarantee that the conversion into date-time of the same program on two different computers will give the same result, because of the process of double following IEE754.

-- On the same computer equality of numbers is not guaranteed unless bitwise comparaison (see how floating numbers are dealt with).

One generally accepted solution in the industry is to define time as a struct with a int64 for the standard Unix-time with seconds granularity, and a int32 for fractional seconds for example. C# has taken a simpler approach.

#### 1.1.3. Time in Csharp

This part describes the last available functions of C#, that is this one delivered from dotnet version 7 and does not deal with preceding versions.

- Under the hood a DateTime variable is a long int, that a signed 64-bit integer. This allows for precision to the 100 nanoseconds from 1 Jan 00 to 31 Dec 9999. 100 nanoseconds = 0.1 microseconds. This maximal granular fraction is called a ***Tick*** in C#.

- Construct a DateTime object consists of entering a date:
<code>

var date1=new DateTime(2008,5,1,8,30,52);
</code>
that is:

*DateTime(Year,Month,Day,Hour,Minute,Seconds)*

One can also enter directly the Unix-style *long* like <code>DateTime dateTimeWithTicks = new DateTime(637825507520000000);</code>. A Console.Writeline of that var will only give seconds precision, hiding the precision included in the original variable.

What if you have fractions of seconds?
<code>
var date1=new DateTime(2008,5,1,8,30,52).AddMilliseconds(562);
var date2=new DateTime(2008,5,1,8,30,52).AddMicroseconds(562568);
var date3=new DateTime(2008,5,1,8,30,52).AddTicks(5624582);
</code>

To know the complete value of a DateTime variable, the only way to see the complete value of a DateTime object is to convert it to string (!).

A DateTime value can be entered with specified *Calendar* and/or *DateTimeKind*:
*Calendar* is what is used to interpret year, month and day. Default is GregorianCalendar (ours), but there are JapaneseCalendar, HebrewCalendar, PersianCalendar,...
These are in the **System.Globalization** namespace.

*DateTimeKind*
It is a pre-time zone specification. It has 3 possible values:

- Unspecified (default)
- Utc. Means the DateTime is in UTC
- Local

This influences the conversion to utc.

***Time Zones***

The DateTime object does not use time zones. For this another object exists (use is mandatory):

DateTimeOffset
<code>
    DateTimeOffset dateTimeOffset = new DateTimeOffset(2023, 12, 21, 14, 30, 45,456, new TimeSpan(2, 30, 0));
</code>

- ***We can enter directly a date with a precision of millisecond !!!***, which is not the case for DateTime.

- There is no way to enter a DateTimeOffset precision higher than millisecond, unless converting to Ticks (!!!!!). It is simple though:

<code>
    date1.Ticks
</code>

*** Measuring a time lapse - a duration ***

We use a new type of variable: TimeSpan.

TimeSpan is very simple: TimeSpan(Hours,Minutes,Seconds)

You can add and substract a TimeSpan to a DateTime or DateTimeOffset.

#### 1.1.4. What a mess

Unfortunately, excepted in Go language (but with manipulations under the hood), the other languages are not better. The main pro of Dates and Times in C# are the functions of additions of days and months and the large parsing methods available. Consequently he only accepted format for the time in this library is the RFC3339 format.

## 1.2 Measure

In each Observation there is a Measure (it is the point of an Observation). Although many types of data can be used, we only deal with floating numeric value, as it is the most complete and easy to deal with.
What if there is no value for Measure (and therefore for Dmeas)? The topic of dealing with **missing data** is a very special topic, that is mostly ignored in most of the statistic libraries and programming language I know.

Two solutions are implemented in the industry:

- An improbable number. It means not zero. The earlier languages used something like -99999.999. The problem is that, being normal values, they do not raise a flag and can produce false results. The most sophisticated is the "Not-a-Number" and the NaN boxing it gave birth to inavertently. Better know as "double.NaN" in ISO754. Works only for floating numbers.

- The addtion of a field to the number that indicates if the number is valid or not. Implementation in C# in one of the most misleading name: isNullable is actually the copy of a double into a struct with a IsValid bool member.

- A third solution is possible: working with pointers on double. It presents two problems: if the language does not accept null pointers, dealing with null pointers in exceptions notwithstanding the fact that they are correct values. Finally, I am not sure that the frequent referencing/dereferencing is good for the performance. And the coding is less easy to read (not a correct argument)

## 1.3 Dmeas, Dchron

Each Observation has a Dmeas and a Dchron member. Once the Observations are sorted chronologically, they will contain the difference of measure, respectively time, from the preceding Observation. This member variable has therefore no sense outside a TimeSeries.

# 2. TimeSeries

The data unit **Observations** are grouped in a **List**. They are a field of the **TimeSeries** Class.
In addition to the List, a TimeSeries has a *Name* and a *Type*, and a member of type SummaryStats which is a list of statistics computed.

There are only two Constructors: default and **TimeSeries(string name, string type)**.

TimeSeries Methods: How to use the library.

To work with a TimeSeries, firts create an occurence:

<code>
TimeSeries ts = new TimeSeries();
</code>

The class is mainly filled with unitialized variables.
First operation consists of filling the List *Observations* in any way available. For example *Simulate* to produce simulated values; *SQLimport* for fetching data from a database.

Once the TimeSeries is build, most of the time the method *RunStats()* must be called. It compute many things, amongst others the Length of the data series, and fill the Dmeas and Dchron members.
The method has been made to traverse the List as few as possible.

*Notice* There is a concern to traverse a (long) list as few times as possible. This requires to re-code the statistical functions that are found in the standard libraries. For example, in MathNet.Numerics (as all the others I know of), a function like *Mean* can be called, taking a  (NaN exempt) array of double. The computation is rather simple and consist of traversing the complete array. If i need to call another fonction i will traverse againd the array. Since we have more than 30 functions to call, it is an interesting feature to compute them "en passant" and traversing the list as few as possible. Furthermore, a lot of statistics return an *Observation* and not a number. For example, when returning a Maximum, i am interested in returing the complete observation (with the time as it happened) and not only the maximum value.

After the building of the completed TimeSeries can the real processing of the data begin. The differents processes can apply in any order. Typically we would have the order:
Warning-Clean-Regularize

But we could have
Clean - Warning -Regularize

Or, with big data length:

Regularize - Clean

Or:
Warnings - Clean on Warnings

Even:
Clean - Regularize on Rejected

## Methods

### Cleaning

There is a collection of cleaning methods. Cleaning a TimeSeries consists of creating two new TimeSeries:
one of Type "Accepted", another one of Type "Rejected".

The Methods are:

*RemoveInbounds(double min,double max)* Reject all data that are below the minimum or above the maximum

*RemoveOutbounds(double min, double max)* Reject all data that are above the minimum and below the maximum

*RemoveQuantileOutbounds(int Percentile)* Same as *RemoveInbounds*, but the minimum and maximum are the percentiles calculated on the entire time series length of valid measures.

*RmZScore(double level)*The minimum and maximum are calculated as a multiple of the Standard Deviation of the time series length of valid measures.

### Regularize()

### Warnings

Why create a collection of TimeSeries ? Maybe keeping the same List and adding flags would be more efficient ? I did not chose this option for two reasons:

- I would have to imagine a system in which the chaining of operations could be unlimited and in any order.

- Any solution approaching this is too complicated (it is an argument here) and would require selections SQL-like post-processing which the complete data would be traversed a number of times.

## Exportation

Under the form of a Dictionary
