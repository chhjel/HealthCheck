export default class DateUtils
{
    static IsDatePastDay(date: Date, day: Date): boolean {
        return date.getMonth()     > day.getMonth()
            || date.getDate()      > day.getDate()
            || date.getFullYear()  > day.getFullYear();
    }

    static DatesAreOnSameDay(dateA: Date, dateB: Date): boolean {
        return dateA.getMonth()    == dateB.getMonth()
            && dateA.getDate()     == dateB.getDate()
            && dateA.getFullYear() == dateB.getFullYear();
    }

    static getVuetifyTimeFormat(date: Date): string {
        //@ts-ignore
        return `${(date.getHours().toString().padZero(2))}:${date.getMinutes().toString().padZero(2)}`;
    }
    
    static getVuetifyDateFormat(date: Date): string {
        //@ts-ignore
        return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padZero(2)}-${date.getDate().toString().padZero(2)}`;
    }

    static FormatDate(date: Date, format: string): string
    {
        let _pad = (n: any, c: any): any => {
            if ((n = n + '').length < c) {
                return new Array((++c) - n.length).join('0') + n;
            }
            return n;
        }

        let wordReplacer: any = {
            //The day of the month, from 1 through 31. (eg. 5/1/2014 1:45:30 PM, Output: 1)
            d : function() {
                return date.getDate();
            },
            //The day of the month, from 01 through 31. (eg. 5/1/2014 1:45:30 PM, Output: 01)
            dd : function() {
                return _pad(date.getDate(),2);
            },
            //The abbreviated name of the day of the week. (eg. 5/15/2014 1:45:30 PM, Output: Mon)
            ddd : function() {
                return dayNames[date.getDay()].slice(0,3);
            },
            //The full name of the day of the week. (eg. 5/15/2014 1:45:30 PM, Output: Monday)
            dddd : function() {
                return dayNames[date.getDay()] + 'day';
            },
            //The tenths of a second in a date and time value. (eg. 5/15/2014 13:45:30.617, Output: 6)
            f : function() {
                return parseInt((date.getMilliseconds() / 100).toString()) ;
            },
            //The hundredths of a second in a date and time value.  
            //(e.g., 5/15/2014 13:45:30.617, Output: 61)
            ff : function() {
                return parseInt((date.getMilliseconds() / 10).toString()) ;
            },
            //The milliseconds in a date and time value. (eg. 5/15/2014 13:45:30.617, Output: 617)
            fff : function() {
                return date.getMilliseconds() ;
            },
            //If non-zero, The tenths of a second in a date and time value. 
            //(eg. 5/15/2014 13:45:30.617, Output: 6)
            F : function() {
                return (date.getMilliseconds() / 100 > 0) ? parseInt((date.getMilliseconds() / 100).toString()) : '' ;
            },
            //If non-zero, The hundredths of a second in a date and time value.  
            //(e.g., 5/15/2014 13:45:30.617, Output: 61)
            FF : function() {
                return (date.getMilliseconds() / 10 > 0) ? parseInt((date.getMilliseconds() / 10).toString()) : '' ;
            },
            //If non-zero, The milliseconds in a date and time value. 
            //(eg. 5/15/2014 13:45:30.617, Output: 617)
            FFF : function() {
                return (date.getMilliseconds() > 0) ? date.getMilliseconds() : '' ;
            },
            //The hour, using a 12-hour clock from 1 to 12. (eg. 5/15/2014 1:45:30 AM, Output: 1)
            h : function() {
                return date.getHours() % 12 || 12;
            },
            //The hour, using a 12-hour clock from 01 to 12. (eg. 5/15/2014 1:45:30 AM, Output: 01)
            hh : function() {
                return _pad(date.getHours() % 12 || 12, 2);
            },
            //The hour, using a 24-hour clock from 0 to 23. (eg. 5/15/2014 1:45:30 AM, Output: 1)
            H : function() {
                return date.getHours();
            },
            //The hour, using a 24-hour clock from 00 to 23. (eg. 5/15/2014 1:45:30 AM, Output: 01)
            HH : function() {
                return _pad(date.getHours(),2);
            },
            //The minute, from 0 through 59. (eg. 5/15/2014 1:09:30 AM, Output: 9
            m : function() {
                return date.getMinutes();
            },
            //The minute, from 00 through 59. (eg. 5/15/2014 1:09:30 AM, Output: 09
            mm : function() {
                return _pad(date.getMinutes(),2);
            },
            //The month, from 1 through 12. (eg. 5/15/2014 1:45:30 PM, Output: 6
            M : function() {
                return date.getMonth() + 1;
            },
            //The month, from 01 through 12. (eg. 5/15/2014 1:45:30 PM, Output: 06
            MM : function() {
                return _pad(date.getMonth() + 1,2);
            },
            //The abbreviated name of the month. (eg. 5/15/2014 1:45:30 PM, Output: Jun
            MMM : function() {
                return monthNames[date.getMonth()].slice(0, 3);
            },
            //The full name of the month. (eg. 5/15/2014 1:45:30 PM, Output: June)
            MMMM : function() {
                return monthNames[date.getMonth()];
            },
            //The second, from 0 through 59. (eg. 5/15/2014 1:45:09 PM, Output: 9)
            s : function() {
                return date.getSeconds();
            },
            //The second, from 00 through 59. (eg. 5/15/2014 1:45:09 PM, Output: 09)
            ss : function() {
                return _pad(date.getSeconds(),2);
            },
            //The first character of the AM/PM designator. (eg. 5/15/2014 1:45:30 PM, Output: P)
            t : function() {
                return date.getHours() >= 12 ? 'P' : 'A';
            },
            //The AM/PM designator. (eg. 5/15/2014 1:45:30 PM, Output: PM)
            tt : function() {
                return date.getHours() >= 12 ? 'PM' : 'AM';
            },
            //The year, from 0 to 99. (eg. 5/15/2014 1:45:30 PM, Output: 9)
            y : function() {
                return Number(date.getFullYear().toString().substr(2,2));
            },
            //The year, from 00 to 99. (eg. 5/15/2014 1:45:30 PM, Output: 09)
            yy : function() {
                return _pad(date.getFullYear().toString().substr(2,2),2);
            },
            //The year, with a minimum of three digits. (eg. 5/15/2014 1:45:30 PM, Output: 2009)
            yyy : function() {
                var _y = Number(date.getFullYear().toString().substr(1,2));
                return _y > 100 ? _y : date.getFullYear();
            },
            //The year as a four-digit number. (eg. 5/15/2014 1:45:30 PM, Output: 2009)
            yyyy : function() {
                return date.getFullYear();
            },
            //The year as a five-digit number. (eg. 5/15/2014 1:45:30 PM, Output: 02009)
            yyyyy : function() {
                return _pad(date.getFullYear(),5);
            },
            //Hours offset from UTC, with no leading zeros. (eg. 5/15/2014 1:45:30 PM -07:00, Output: -7)
            z : function() {
                return parseInt((date.getTimezoneOffset() / 60).toString()) ; //hourse
            },
            //Hours offset from UTC, with a leading zero for a single-digit value. 
            //(e.g., 5/15/2014 1:45:30 PM -07:00, Output: -07)
            zz : function() {
                let _h: any =  parseInt((date.getTimezoneOffset() / 60).toString()); //hourse
                if(_h < 0) _h =  '-' + _pad(Math.abs(_h),2);
                return _h;
            },
            //Hours and minutes offset from UTC. (eg. 5/15/2014 1:45:30 PM -07:00, Output: -07:00)
            zzz : function() {
                var _h =  parseInt((date.getTimezoneOffset() / 60).toString()); //hourse
                var _m = date.getTimezoneOffset() - (60 * _h);
                var _hm = _pad(_h,2) +':' + _pad(Math.abs(_m),2);
                if(_h < 0) _hm =  '-' + _pad(Math.abs(_h),2) +':' + _pad(Math.abs(_m),2);
                return _hm;
            },
            //Date ordinal display from day of the date. (eg. 5/15/2014 1:45:30 PM, Output: 15th)
            st: function () {
                var _day = wordReplacer.d();
                return _day < 4 || _day > 20 && ['st', 'nd', 'rd'][_day % 10 - 1] || 'th';
            },
            e: function (method: any) {
                throw 'ERROR: Not supported method [' + method + ']';
            },
            T: function () { return 'T'; }
        };
        
        const dayNames: string[] = 
            ['Sun', 'Mon', 'Tues', 'Wednes', 'Thurs', 'Fri', 'Satur'];
        const monthNames: string[] = 
            ['January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'];

        let wordSplitter: RegExp = /\W+/;
        var words = format.split(wordSplitter);
        words.forEach(function(w) {
            if (typeof(wordReplacer[w]) === "function") {
                format = format.replace(w, wordReplacer[w]() );
            }
            else {
                wordReplacer['e'](w);
            }
        });
        return format.replace(/\s+(?=\b(?:st|nd|rd|th)\b)/g, "");
    }
    
    static CreateDateWithDayOffset(daysOffset: number, zeroOut: boolean = true) : Date {
        const date = new Date();
        date.setDate(date.getDate() + daysOffset);
        
        if (zeroOut == true) {
            date.setHours(0);
            date.setMinutes(0);
            date.setSeconds(0);
            date.setMilliseconds(0);
        }
        return date;
    }
    
    static CreateDateWithMinutesOffset(minutesOffset: number) : Date {
        const date = new Date();
        date.setMinutes(date.getMinutes() + minutesOffset);
        return date;
    }
}
