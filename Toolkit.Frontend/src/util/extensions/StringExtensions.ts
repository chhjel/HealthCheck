String.prototype.padZero = function (this : string, length: number) {
    var s = this;
    while (s.length < length) {
      s = '0' + s;
    }
    return s;
};

String.prototype.trunc = String.prototype.trunc || function (this : string, n: number) {
  var s: string = <string>this;
  return (this.length > n) ? this.substr(0, n-1) + '…' : this;
};
