String.prototype.padZero = function (this : string, length: number) {
    var s = this;
    while (s.length < length) {
      s = '0' + s;
    }
    return s;
};
