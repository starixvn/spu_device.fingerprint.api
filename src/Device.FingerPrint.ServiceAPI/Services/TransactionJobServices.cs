using Device.FingerPrint.Features.ServerTransaction.Services;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ITransactionJobServices
    {
        Task<bool> UpdateTransaction(int batchSize, int numberOfBatch, int retry);

        Task<bool> UpdateTransactionRetry(int batchSize, int numberOfBatch, int retry);

        Task<bool> UpdateTransactionN(int batchSize, int numberOfBatch, int retry);

        Task<bool> MergeTrans(int batchSize, int numberOfBatch, int retry);

        Task<bool> ForceMergeTrans(int batchSize, int numberOfBatch, int retry);

        Task<bool> Evolution(int batchSize, int numberOfBatch);
        Task RatioAsync(string url, string a, string b);
    }

    public class TransactionJobServices : ITransactionJobServices
    {
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionServices;
        private readonly ISentryClient sentryClient;

        public TransactionJobServices(ITransactionService transactionService, IRawTransactionService rawTransactionServices, ISentryClient sentryClient)
        {
            this.transactionService = transactionService;
            this.rawTransactionServices = rawTransactionServices;
            this.sentryClient = sentryClient;
        }

        //public void ParseAttributes()
        //{
        //    var str = "[{\"audio\":124.04347527516074,\"duration\":33},{\"canvasPrint\":{\"winding\":true,\"geometry\":\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAHoAAABuCAYAAADoHgdpAAAAAXNSR0IArs4c6QAAF9hJREFUeF7tnQl8VNW9x79nJgkkKEXZBAWEoEEF25KkrVo3tNQqIu7iWpcyaK19Vqt9XZ7axS6v7euzKoZWrdZqtVrXal/daF1rElBBCwJBUEAU0acsITOZ08//3plkMnPn3nPv3FlE/p+PH21zlv/5/+ac+z//7SjKSLpBj6SLRhQT0TQQYQyakcCeedhagmYtiteB19AsooZ2tUSt0b9tH0+3nghMQjMRpXcDVQcMsP+tB9j/bdEmUJtAb7b/W29GqzdRLAIWElWL1HmNyxq0HtkFjQomamiIwBiNO38a1ips/jQsqoH2JUqtKaOYralVKRnQ43U/kkxDMxXFIS6AurOV3ALdm6B7s/3PTps1Y7ViD2ACEA22qi7gCeAp4Pmo0q/vXKcYUgfD6mDoANi5NtjANujzFPytPzz4qlIyVUmpJEDrcXoaMBM4NdDqBMzE+zaoSfkn7j7MJ4F9U6AbTCjgPgg84NW2thqGCuh1MGYQ1o8gGN0O3NGh1EPBuvvvVTSg9d66hk4uBGYBDb5ZS3bZ4Cb+HxIf+O5udRgKNAOfy93l8lO5Ffgj0BFsdNh1IOz+CRv0HWqCjLIEmNsfri32Lg8daI1WjONbaC5BMdj36gXc+Hs2yDrpu7tjB9l4nwcOQrjjBuC3wPvhjA5V8vUeBON3sv/tkzS8q+AXHfATlNI+uxs1DxVoXa+/jOZKYIzR7JmN4hsg/m7w3Wsw4T07wTVT4M3JBo2DNpFdPmEw1O8cZISVCq5crtTvgnR26xMK0HqsbkDxc0C+xf4ovj4F8EZ//Xy0lqP5xylFy+omCtuXgCE+BvHbdJcdoGEwNASa5CENl65QSo72UKhgoHW9noXmWqDaF0dyNHetg0TxABZ+7gCuAnLUN9HMj059w30x7rOxAP7J4UGO9LiCry1XqsXnjI7NCwJaj9UtKEvZMqdkpw1w13rzPgFbfielbLl2F2VtRsAJ/HSTnS2AD+rvpxca5q5QKuark0PjQEDrMXoEEW5P3YXNeeh6C7auA50w7xOg5dvAfwD/NO07FjgJGGjaIWC7/lU22J/cxdcAcgdPwGlvFGB48Q20rtdixbrb15VJ7r9bVxdV0UpL7jXgq0GuTHIVk5v+cF8YBGssCttnd/V7D1+i4ITlSon1zjf5AlqP15NJIpf8EcYzibLVubrou1j4eQU4F3jHmLmshrKjz8A2chabZHcL2P6UtbURmLZMqfl+2TMGOrWT/2YMstyBZRd3yUFafJKdfGYhIKdZFLC/XKKdLXPuM8wGXO7iZiS29Kl+d7YR0HqUHkm1ZQY2s3CJLbpzVdE16rRc5Kd0WpDjOp9g5RiXo2FHM8kX3Eo088+P9mNLX9INh65Uaq3p3GZAj9VPGite4mzYsgKSW015KLidGNCNFS/T2URBO8+0cQjtBvaDKWNhWNrB5j6mKGgrlDrUdGZPoH1dobo/hE1yiJaOjK5QQdkp1dUrk79pe8JIs6PEz9XLFWhdr2NoyzTsTWIA2bzcu12ILcQY8t0Qx3McSu7YAngpaWo97G5mM1cQW67UXC/28gKdMmsuNLJ4bV0DW40/F148Gf1dzJpixSzujTzl9bqoyOZSpxU3joBGI/U/rmGSl7k0P9DjtLhovW3XYuXqfNMInDAbfSXTdh3mwE5jiW1crl2lpv12g0lGF/uHOpQSg25ecgQ65YW62XNdckfestKzWdgN7gEuC3tQr/GOB4rp9co3/8FjjO7aCs5283rlAJ3yJ6/wdDUm3oPNgV32XmLN+3dx1h4MrA48QsCOOwGXlDr4KsXrF8bBWGHAlVZ2wNh8/uxcoMfp/wSudh3SukItg2TRv5A5bMwByx9aFpqa+pWVevLaKjhivB235k7f7lBKPLI51AdoK/xnC2tcI0PE4rVlacmMIZkcS0Td/sB7Xsst1t8lUuVbwYMPC2JLjCpH7uFqQZNIlVoY6RSW1BfocfobwC9cGep8o2RmzWw+bvQ8agoSpVnnI4EDzJqG3krMpQeM8hr2kg6lfpndKBvoxa5mzjIpX2mmDwdEeSgriXlUfKDlIm/lbEmHUnJP6EM9QKdCcuVK5Uziaty8tCReKCcGxNAuV6qKILlq5YiyRJyJ10uOcPdQ46OzQ4kzgf6Da9y1gBw07DYEGVxsEncdwjxGQ0jcuAQqlIvEn32UZCvkpds7lBI/Tw9ZQKdisPN7ISQyRHzKZSJRwvYq09x5p5VAtKoyMiWuTZdIlQj0X6ZUD6Y20OO0mAMkaiSXJMZr05KyHdnC0F9TUSNlFGvu1OIy26eMHMkRPr3BLQbthA6lxLZkkQ20W5Bf58qSBPK5iayoHqqgWJXDs5XNq0SniHLmQNmerfSOlvjh3AzGMniknJiuCG07mzEJ1xbFodz0xfp8ocSvdSjVEyiirNTVeB6L4uYlZTGMZMpuXcpIUm55Os4vxhMz13Hx2BdDihzhDlQNu6ZTdpUeq49GOSQSlvnOnOb78VSWXvEkVcDI5bxmZbKd526tYfoKpawrs9L5bNsVsJuFwbLatr1+A+WyfWfzlX9X99i+BWhxR0rcYy9JwpvEfVUAiTuyR3WsAH76sCBuS7mvVAIdNjYnsU/DLSuUsrCVozs38K/MxpFMuRUl8C8sYMalokXDGq+QcRyMKArmLU8FEMqO7mvfrhBNO73mL4QZxluIIJ36VormneYtVwPv0bwF6L6J13Jky9FdIVRfIXzkZeNHFcSg5GTLEZ5BHUpZV+i+QEs5iU2vhFdpIAQZbAfahxAl2+OkffqU2XAGWtJnxN9cQbQdaJ9giL9a/NYpcga6gpSwNKO+gFbvQdVaiK6B6FsQXQcfSKx5PofMrjCwHrqHQ/cu0D0SEiNAe8Zn9Uq/ko5u4SpLKcsFWvzNm/7l8+dT/ObuQMch+hJEF0HVQlCrIJmEriR0+yx0E41ATQQiEdCjoXtfSOwD3eKTdCnmUGlACyTH7dXjr84FugxB+CY/E0egq16Bqmcg+ihEFSQ1xLuhqxspEVAQiepSE4XqKEQUdGvo/gIkDrCBz6ZKBDoj+D8TaNuhsWU5xEMryFSQrDM79zo0ElA9D2r+CtFVvU0E4K3dNthhkoDcLwV4etzu0dB1BMQl4FiKy5U5rCjfeiWdR9J6oCesSOl6/SSaQ9j4sndFvjAFaTjWqWj+Wf0wVN8LVVmF5QTgrUUOOe5XZQOeSYmBED8WdjsSzvPMUzRcaYjN6qrh9H3lcOvJuBTL2O/QnWexUeoFVBhVPctlNXdwT5X4sLJoU5d9rJaC5PMwwKEy4OHD4eiZsEaCkCuM5Jo1qP/vOpQ6WziznRrxDVdXim3bEld0PdTcBtXPOjs1NnaFf1R74SRHeXYZyEuB8+WA3B8Wng4bAtUU85o52N8l13r8zhlODXFTbl31AF2BK38EYyRfr6q/Q/8WiNhHco6bckvCVrzKQaKgSdZEmiRZ9bDU//gwCv+YDavl+10BtM9Q9AGjM9yUEnjw/uLVVlnkclP1TVArEWK91CfwQLTqziJ/k71kILFaopULPQf02ibs/+/xI2D5OV6jFP/vQwdQfeyE3sADmVFPaE+yXsqhlos2QM110F/SsXPJ0rxFq94cL/2Rnc2OHOGi7NQreCyPvJ6dBIukCFageqChgLB7VOknzmvsqYCjrMr19+iltIYyfoBBVkHt/0J1ftOrFRwo2rVo2ZVAooWfVQVud+hlo+CJrwOjy8LxKcCPomoPeUlAGFC6pW0Gr3AvUiq85NQBO/wcIu7lIv+a1HxVtOwSKdmeYpCz77oaONHjENw4BG4XjU0c16Wl64Aj4FgVa7ovDfT3SPB9rigtI7AKdvwJKO+aoF2dCfaS73Ml0Z+jcKBBBH/nELhVoghLu7MlyKAa/kvFmn5gA31D250oTuIu4KVSSXIDDPgxRA2rJWyKc3F30vsphFKxL+bvCyMww7Cg8boxcL+knZfmmz0d+B+RheYuNbvp5NSObn0G1P7IT+D3JZJU7dVQ/aLZZKKEbeyyqtlVXJLdaTUwwFCHfe1TMO/bZmsusNVvgCkWujyjZjXJ2wPWN3oB8Clr7F+FUWPRg8t+t0C/v5gvJS6J93a17YoI5M+0b0+phvHGpR2h9ShYcJb52gO0FG3g0d5+L6pY06fTQEsFODs17xng4QCjm3apfhpqrzFtbbeTe3Pq+1xxifCTorCfwXc6c8WPXgQrrE1WFJIzQ6pbpmiZijVZ2Crd0v4m6F2tP4i+8xNAnv0Km6LvQt2FoHwqVbKbZVenqujvV0mlLXaPwFTD73Ranpuq4JFfwwb/78p4QSIl6J7v6z1/S8WarErMcnRLSZDeMnV/B6SGb9hU+yvLdu2bspwXZQ3ozw7YHxKB43wCLQL41/7wVPhlE9Km9wwZb1SxJitpSICW9ONebuWuKlVMwqwIU/Us1IkCEIA+lPtz7wW6ospP9VNwVqD3ruD+r8O68IqhyJEsezRbNVSxplQUaDbQgoWU/Q4tPUJD3UXg5Go0wT0LaOlSMQXlahRMiMDLPj9HsohdRsD0F0IrXOZUUE63tF6hYs2Ssu9wdKeFL1ctuXIVStV/gdpbgo+Sx+9cESUiBys4Xp4Z7Yb2AM6Wwy+GcVKlrmDKKRGp57ReSYTvqlizpS32VcYy5xODlSjIAX6svcPEoe783MgQP+vKUMYyu0nNQqkE5fFKpZ+ZnNuKoypf0ddMZaytG+b7BHvIIDhObrcBvvMZQs4u+mqDrK4A9Z6KNVpWGvlG916vspcqjg7LUhqQqh+DWs8Kw+6DZ1yvshuWvYxz9vWqNQELfO6MA78Je4nzIxgpmJ35NlYvyNZ4q1SsySqJ0Ndg4jSXAB3UszXg0r6BfEHWkmEwcepe1LIXXuUrnAwmLyTgRR9gDxkFx4lj2z9ll6/IAlkGXKRiTZNSQKdMoG7zyIudfrNoJSS3ztIDCqOUCdRtkKJkXJo8tZDPBPrPBLzkA+xpN8BI74rZmTLIfmrBAWQxdj+nYs1WQFuvU8NNkh8CYpbyE23UrwX6SSBQCLQp7hqQX5THUyRIxO3Bs2EeTo3nE+ba+OQjocnXJ25JHKakHzxzBjnHqdH2PeD7nnBITI+8gWr0lHMcBswEyX4Ig1y+0+nhS/4ckon587kELDTY2cMjcIzEBxjdyfs8h5QXZFswGW5KCTyAe43wWJPycHmBHWmDHX5mNKRRIzm+DQIPSvbAmWjiM2pArlde9GwCFhmAfcocGOhaRF9m6vPAmQfI0j4j8EBCibr1Ui9+e/4uO1vUXbdjvOZm6P+I8ZBGDQ1DiUryZOG+UficD2fGMwl4xQPsg06ECZYXOR/1ebLQAGRJV+oNJZJRdUvrFlDmz6DKjpZAhXwKWt0lUBVy+q2P4MBAj5CKe96klFStgmOrYQeD3ZwJ2dMJeNUF7NFj4AhxH+aSKF5JODX9oJkRyOhOFWuuTY9mF5RraX8BtP9Hf5yuXpK6umPBr+A6/6p9hvsaXb28rlDZnOxfBROzUnSMjiTAC+xz2yDa9yVagytUntlVq4o1fiYL6NZrQUl8qn+SO7ZUskr/WMO6VuXjxGcAv3xlrnR6NinIQ+H1UTjMx5HttAa3b/ZRc2DXnu+0PBR+YeabVmY7OT2pvk7Fmi/MArpNrqJSxjkYiblUPslWRNojUOv9wE6wiVK9fKbkiLlUHpqQcCSLpNa2PJrlJ4NGjupTjbRi76Xlu3oddjHUW7bvhzRcmvmWlT+QLRZOU7Gmnthe++j+TfteJPWr3hx6tBCv19MtXWx5PCSJuMwXIMnu7p3gmimw2u+zRmnnRcECyhjAwYI2bP9pW9+ZeIOYNOUi20MBQJbc7r3VVxp7Khv0FmZvaZP6D0ZPp7mud/IFy2hdPx6xphU73do0bVYePZHonYNA8lHkDUZj9pqqYHLAb7LXDyPl9ZKoD3nvdPbEUa9HDni+T1mhQCDDGhVrsqOGUtQL9A2t96HUMV68ef79kDPXsGenFJKFWyXFosiFwtwS4SWQT5Stz+W+bOPJnmjXn4lCQ5FAToX1n7KgmzNbE7b/as8h76tDX+opoBIQZAnUuF/Nbhb7iAPQPa4tTyjdGxx9AozIsojJx1EUtgcKHDtf9+zSFhJ3va/5uxd92BNc94namrXfK5Th8iTuWlQuKyRXSOziYh/fPYL64hv257QQPJL6KnV+s+igjkB/moj/J+Vz1nbyifCJPHdM2UZi/n4q9eCzX0dJPkHKYfdZCVrWMLwb3knCuz7zdwYr4iMjPL5nlKcGK+s96rDZOzAFrqMCI9az5UnUjDdUQSCLjJJ6sjq/WRzduUBbv6K5bX9DI1UZg9PZJ5n70cWyIQmUYs4SqcpbpmJxy/cSogAqb3KKJiEBzFJKXpxw2amrwv2HGtamAJf/tv5JLUsMIzsq+x9RtOQEkv/OomKy5yhgMaj84+6r7KCBgKR4VM1qkjDGPtT33auWVonH/XXAKexufoAuaKJtsLM8dXKLmBwLIf01FWu+1h3omxaOIr5VbsOipwYjt6M72Igfn17va7jrT4WsdzPV/Saocybl2J9zHyFtab0DlKTXBiMnZSzYSB+/XvKpedD5sSIzYeg/qljzTKe2uUDPnT8dnbzfbGCHVgefsZaGrVZ2wHbyKYHX+q9h3q3BbRkqcoyaNdnxbuOoHheklInBpGn9eJ9L3N5cJDB/8FLa5rg+UZdXUHmUsHT7PEDPPwWdFH+Afxr/nUVMWTrRf8ftPXhyz5dZ+kOxAPgnFZmpZk0W85Qj5XWq6rltT6MDPKA7rGU+Mx73a032v7Btscd9h83n7Zh/2WXkQQcAuvUctJKQQH9U9+QCTp9j5eRuJ58SuO38BWw+1L/slD5XzWq+yW021zCJYLv63S5mnV9875VPGX4kms+d0wWD/cnOYDfL2j2ADqiBH376asZ19fGefCQEXU4mO2pW89ht/mXmomlnLscz8Em3tN8I2l8pvN1+sJgjF5brKe1ywhV87ocnLebN7/mUmbpJxRozChzkn94b6BtbG0iop33FY1TPe4uzr+8b/BRcBB+Pnjdf8BbxQ/zIbD1V+vPq3Gapt+5JnkDLCLql9TJQP/UcradBHI6ZCRKYvp28JbAuCffLzchPPJq+XMWajYPnjYC2wPbr2drr8pc5cEWwO6G3aLatFk+NfZl//dRcVh7GESfhmAM9p1X81VLdxCykrv+8hZx5vZXJt508JHDrBQvpPMRUVutJ6qnZ/mYvGRsDbe9qn3fr6aetZJe485PlXpx9XP7+VvVKHviDuYwM7swF7eh0Z93S/mvQPfHCrngMu7mNGY80fVwwC7TO+77UxttnG8pIXatijV8LMo+vHW0rZm11KCQSxaCkTgK+dNoHjNJuCahB+N42+ryhPuCRPww0UsIUz6CZqmJNgarA+QbaAvuG+U2o5EOpwB53oe9846uc8H97bxvIhLyKu7/4KhvONZHNOnRkmpo9uS0oB4GAtsFuPw6lDYpUaZhy+juMj0vw7XZKS2BZ9Ts8cdtQD+Ok3Vqr49Xsxj8XIrzAQNvK2fxz0Elvx8cODyzm1Nt8Wn0KWdZHoO/tpy9m43RvmajIuWrWZFeHhclqCwLa/ma3Xgzql56TfearL/GpdyTieju9OPQlXrjOQBb6GyrW7Jo0bSrMgoG2wDYJNq96fQknX1bPAF/mH9N1fHTabSLBnT9bTmL3BlemHYLwC1lkKEDbx3j7sWjt/h0ZdOd8TrrHv2O9kBVWWt8/HT+f9052l4FSx6lZjWblRgzXFxrQ9jHedmhmdqojD+OvXMCUV/071w0XVNHNnth7Acuu9Fr7FBVrejLsdYQKdApsSY6RXOv8Tz/vN2sxk973VkTCXm05x1s4aDHPzXVbs7xoLjnNkg0UOoUOtAX29e37UqWvz29U2QDTLlrFyK7SPh0TuvgMB1xTs4qHrhmd9/EUMYYk1AXqgsaXDUf03awoQKd2dp3t2sxnLl0FZ3xzHbVasqm2Xdqi1vH7/x6e/zkkdS2IyzGYxctUcEUDOs1AyhEivmwHr1cHzPz22+yYdEqTM11D5bb7MPI2d1w9LM8DZ+tR+nKvoL6wFld0oK3dLS7OqPqpc6bmKjjou2uZ0LltZXcs7r+Wf/xwhONOVjxKt77cr6uxEND/Dah+mtivWQcwAAAAAElFTkSuQmCC\",\"text\":\"\"},\"duration\":88},{\"colorDepth\":24,\"duration\":0},{\"colorGamut\":\"srgb\",\"duration\":0},{\"contrast\":0,\"duration\":0},{\"cookiesEnabled\":true,\"duration\":1},{\"cpuClass\":\"undefined\"},{\"deviceMemory\":8,\"duration\":0},{\"domBlockers\":\"undefined\"},{\"fontPreferencePrint\":{\"default\":149.3125,\"apple\":149.3125,\"serif\":149.3125,\"sans\":144.015625,\"min\":9.34375,\"system\":147.859375},\"duration\":48},{\"fonts\":[\"Calibri\",\"Franklin Gothic\",\"MS UI Gothic\",\"MT Extra\",\"Marlett\",\"Segoe UI Light\"],\"duration\":131},{\"forcedColors\":false,\"duration\":0},{\"hardwareConcurrency\":8,\"duration\":0},{\"hdr\":false,\"duration\":0},{\"indexedDB\":true,\"duration\":0},{\"invertedColors\":\"undefined\"},{\"languagesPrint\":{\"language\":\"vi\",\"languages\":\"vi,en-US,vi-VN,fr-FR,fr,en\"},\"duration\":0},{\"localStorage\":true,\"duration\":0},{\"math\":{\"acos\":1.4473588658278522,\"acosh\":709.889355822726,\"acoshPf\":355.291251501643,\"asin\":0.12343746096704435,\"asinh\":0.881373587019543,\"asinhPf\":0.8813735870195429,\"atanh\":0.5493061443340548,\"atanhPf\":0.5493061443340548,\"atan\":0.4636476090008061,\"sin\":0.8178819121159085,\"sinh\":1.1752011936438014,\"sinhPf\":2.534342107873324,\"cos\":-0.8390715290095377,\"cosh\":1.5430806348152437,\"coshPf\":1.5430806348152437,\"tan\":-1.4214488238747245,\"tanh\":0.7615941559557649,\"tanhPf\":0.7615941559557649,\"exp\":2.718281828459045,\"expm1\":1.718281828459045,\"expm1Pf\":1.718281828459045,\"log1p\":2.3978952727983707,\"log1pPf\":2.3978952727983707,\"powPI\":1.9275814160560204e-50},\"duration\":1},{\"monochrome\":0,\"duration\":0},{\"openDatabase\":true,\"duration\":0},{\"osCpu\":\"undefined\"},{\"platform\":\"Win32\",\"duration\":0},{\"plugins\":[{\"name\":\"PDF Viewer\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]},{\"name\":\"Chrome PDF Viewer\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]},{\"name\":\"Chromium PDF Viewer\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]},{\"name\":\"Microsoft Edge PDF Viewer\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]},{\"name\":\"WebKit built-in PDF\",\"description\":\"Portable Document Format\",\"mimeTypes\":[{\"type\":\"application/pdf\",\"suffixes\":\"pdf\"},{\"type\":\"text/pdf\",\"suffixes\":\"pdf\"}]}],\"duration\":1},{\"reducedMotion\":true,\"duration\":0},{\"screenPrint\":{\"currentResolution\":\"1920x1080\",\"pixelDepth\":24},\"duration\":0},{\"sessionStorage\":true,\"duration\":1},{\"timezone\":\"Asia/Bangkok\",\"duration\":12},{\"touchSupport\":{\"maxTouchPoints\":0,\"touchEvent\":false,\"touchStart\":false},\"duration\":0},{\"vendor\":\"Google Inc.\",\"duration\":0},{\"vendorFlavors\":[\"chrome\"],\"duration\":0},{\"webglPrint\":\"undefined\",\"duration\":0}]";
        //    var newStr = str.Replace("[", "");
        //    newStr = newStr.Replace("]", "");
        //    newStr = newStr.Replace("},{", "}},{{");
        //    var arr = newStr.Split("},{");
        //    foreach(var i in arr)
        //    {
        //        var newArr = i.ToString().Split(",\"duration\":");
        //        var arr1 = newArr[0].Split("\":");
        //        var key = arr1[0].Replace("{\"", "");
        //    }
        //}

        public Task<bool> UpdateTransaction(int batchSize, int numberOfBatch, int retry)
        {
            try
            {
                var rawTrans = rawTransactionServices.GetServerTransaction(batchSize * numberOfBatch, retry, (int)Offset.UpdateServerTransaction);

                if (rawTrans != null && rawTrans.Any())
                {
                    ProcessUpdateTransaction(rawTrans, batchSize, retry, (int)Offset.UpdateServerTransaction);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> UpdateTransactionRetry(int batchSize, int numberOfBatch, int retry)
        {
            try
            {
                var rawTrans = rawTransactionServices.GetServerTransactionRetry(batchSize * numberOfBatch, retry, (int)Offset.UpdateServerTransactionRetry);

                if (rawTrans != null && rawTrans.Any())
                {
                    ProcessUpdateTransaction(rawTrans, batchSize, retry, (int)Offset.UpdateServerTransactionRetry);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private void ProcessUpdateTransaction(IEnumerable<ServerTransactionModel> rawTrans, int batchSize, int retry, int offsetId)
        {
            if (rawTrans != null && rawTrans.Any())
            {
                var tasks = new List<Task>();
                var chunkRawTrans = rawTrans.Chunk2(batchSize);
                var offsetValue = rawTrans.Min(x => x.Id);

                foreach (var item in chunkRawTrans)
                {
                    var task = Task.Run(() =>
                    {
                        var rawTransJson = JsonConvert.SerializeObject(item);
                        try
                        {
                            rawTransactionServices.UpdateRawTransaction(rawTransJson, retry, offsetId, offsetValue);
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + rawTransJson, SentryLevel.Error);
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        public Task<bool> UpdateTransactionN(int batchSize, int numberOfBatch, int retry)
        {
            try
            {
                var rawTrans = rawTransactionServices.GetServerTransactionN(batchSize * numberOfBatch, retry, (int)Offset.UpdateServerTransactionN);

                if (rawTrans != null && rawTrans.Any())
                {
                    ProcessUpdateTransactionN(rawTrans, batchSize, (int)Offset.UpdateServerTransactionN);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private void ProcessUpdateTransactionN(IEnumerable<ServerTransactionModel> rawTrans, int batchSize, int offsetId)
        {
            if (rawTrans != null && rawTrans.Any())
            {
                var tasks = new List<Task>();
                var chunkRawTrans = rawTrans.Chunk2(batchSize);

                foreach (var item in chunkRawTrans)
                {
                    var task = Task.Run(() =>
                    {
                        var rawTransJson = JsonConvert.SerializeObject(item);
                        try
                        {
                            var offsetvalue = rawTrans.Min(x => x.Id);
                            rawTransactionServices.UpdateRawTransactionN(rawTransJson, offsetId, offsetvalue);
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + rawTransJson, SentryLevel.Error);
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        public Task<bool> MergeTrans(int batchSize, int numberOfBatch, int retry)
        {
            try
            {
                var mergeTrans = transactionService.GetMergeTransaction(batchSize * numberOfBatch, retry);

                if (mergeTrans != null && mergeTrans.Any())
                {
                    var mergeTransGroup = mergeTrans.Chunk2(numberOfBatch);
                    var tasks = new List<Task>();

                    foreach (var trans in mergeTransGroup)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            var mergeTransJson = JsonConvert.SerializeObject(trans);
                            try
                            {
                                transactionService.MergeTransaction(mergeTransJson);
                            }
                            catch (Exception ex)
                            {
                                sentryClient.CaptureMessage(ex.Message + "\n" + mergeTransJson, SentryLevel.Error);
                            }
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> ForceMergeTrans(int batchSize, int numberOfBatch, int retry)
        {
            try
            {
                var mergeTrans = transactionService.GetForceMergeTransaction(batchSize * numberOfBatch, retry);

                if (mergeTrans != null && mergeTrans.Any())
                {
                    var mergeTransGroup = mergeTrans.Chunk2(numberOfBatch);
                    var tasks = new List<Task>();

                    foreach (var trans in mergeTransGroup)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            var mergeTransJson = JsonConvert.SerializeObject(trans);
                            try
                            {
                                transactionService.ForceMergeTransaction(mergeTransJson);
                            }
                            catch (Exception ex)
                            {
                                sentryClient.CaptureMessage(ex.Message + "\n" + mergeTransJson, SentryLevel.Error);
                            }
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> Evolution(int batchSize, int numberOfBatch)
        {
            try
            {
                var rawTrans = transactionService.GetTransactionEvolution(batchSize * numberOfBatch);

                if (rawTrans != null && rawTrans.Any())
                {
                    ProcessEvolution(rawTrans, batchSize);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private void ProcessEvolution(IEnumerable<TransactionEvolutionEntity> trans, int batchSize)
        {
            if (trans != null && trans.Any())
            {
                var tasks = new List<Task>();
                var chunkRawTrans = trans.Chunk2(batchSize);

                foreach (var item in chunkRawTrans)
                {
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            var (newDeviceTransIds, evolutionModel) = ClassifyRecoverTrans(item);

                            // NewDevice
                            if (newDeviceTransIds != null && newDeviceTransIds.Any())
                            {
                                var transIds = string.Join(",", newDeviceTransIds);
                                transactionService.UpdateTransactionEvolution(transIds, (int)TransactionEvolutionType.NewDevice);
                            }

                            // FPS Evolution
                            if (evolutionModel != null && evolutionModel.Any())
                            {
                                var transJson = JsonConvert.SerializeObject(evolutionModel);
                                transactionService.UpdateTransactionEvolution(transJson, (int)TransactionEvolutionType.Recover);
                            }
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + item, SentryLevel.Error);
                        }
                    });

                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        private (IEnumerable<ulong>, IEnumerable<TransactionEvolutionModel>) ClassifyRecoverTrans(IEnumerable<TransactionEvolutionEntity> trans)
        {
            var newDeviceTransIds = new List<ulong>();
            var transRecover = new List<TransactionEvolutionModel>();
            foreach (var item in trans)
            {
                var RecoverFingerPrintId = GetRecoverFingerPrintIdByEvolution(item);
                if (RecoverFingerPrintId > 0)
                {
                    var model = new TransactionEvolutionModel()
                    {
                        Id = item.Id,
                        RawFingerPrintId = item.RawFingerPrintId,
                        CookieId = item.CookieId,
                        TransTime = item.TransTime,
                        RecoverFingerPrintId = RecoverFingerPrintId
                    };
                    transRecover.Add(model);
                }
                else
                {
                    newDeviceTransIds.Add(item.Id);
                }
            }
            return (newDeviceTransIds, transRecover);
        }

        private ulong GetRecoverFingerPrintIdByEvolution(TransactionEvolutionEntity entity)
        {
            ulong result = 0;
            if (entity != null && !string.IsNullOrEmpty(entity.FPIds))
            {
                var fpAttributes = transactionService.GetTransactionEvolutionAttributes(entity.FPIds, entity.BrowserInstanceCode);
                foreach (var item in fpAttributes)
                {
                    if(CompareRules(entity, item))
                    {
                        result = item.Id;
                        break;
                    }
                }
            }
            return result;
        }

        private bool CompareRules(TransactionEvolutionEntity newEntity, TransactionEvolutionAttributesEntity oldEntity)
        {
            // Rule 1
            if (newEntity.BrowserInstanceCode != oldEntity.BrowserInstanceCode)
                return false;

            // Rule 2
            var version1 = new Version(newEntity.BrowserVersion);
            var version2 = new Version(oldEntity.BrowserVersion);
            var resultVersion = version1.CompareTo(version2);
            if (resultVersion >= 0) // new >= old
                return false;

            // Rule 3
            if (newEntity.LocalStorage + newEntity.DoNotTrack + newEntity.Cookies + newEntity.Canvas
                != oldEntity.LocalStorage + oldEntity.DoNotTrack + oldEntity.Cookies + oldEntity.Canvas)
                return false;

            // Rule 4
            var arr1 = JsonConvert.DeserializeObject<string[]>(newEntity.Fonts);
            var arr2 = JsonConvert.DeserializeObject<string[]>(oldEntity.Fonts);
            if (!arr1.All(arr2.Contains) && !arr2.All(arr1.Contains))
                return false;

            // Rule 5
            int countRule56Changed = 0;
            int isUserAgent = 0, isVendor = 0, isGLRenderer = 0, isPlugins = 0, isLanguagesPrint = 0;

            if (newEntity.UserAgent != oldEntity.UserAgent) isUserAgent = 1;
            if (newEntity.Vendor != oldEntity.Vendor) isVendor = 1;
            if (newEntity.GLRenderer != oldEntity.GLRenderer) isGLRenderer = 1;
            if (newEntity.Plugins != oldEntity.Plugins) isPlugins = 1;
            if (newEntity.Languages != oldEntity.Languages) isLanguagesPrint = 1;
            //if (newEntity.HeaderAccept != oldEntity.HeaderAccept) countRule5++;
            //if (newEntity.Headers != oldEntity.Headers) countRule5++;

            if (isUserAgent + isVendor + isGLRenderer + isPlugins + isLanguagesPrint > 2) return false;
            if (isUserAgent + isVendor + isGLRenderer + isPlugins + isLanguagesPrint > 0) countRule56Changed++;

            if(isUserAgent == 1 && !CheckRatio(newEntity.UserAgent, oldEntity.UserAgent))
                return false;
            if(isVendor == 1 && !CheckRatio(newEntity.Vendor, oldEntity.Vendor))
                return false;
            if(isGLRenderer == 1 && !CheckRatio(newEntity.GLRenderer, oldEntity.GLRenderer))
                return false;
            if(isPlugins == 1 && !CheckRatio(newEntity.Plugins, oldEntity.Plugins))
                return false;
            if(isLanguagesPrint == 1 && !CheckRatio(newEntity.Languages, oldEntity.Languages))
                return false;

            // Rule 6
            int isScreenPrint = 0, isTimezone = 0;
            if (newEntity.Resolution != oldEntity.Resolution) isScreenPrint = 1;
            if (newEntity.Timezone != oldEntity.Timezone) isTimezone = 1;

            if (isScreenPrint + isTimezone > 1) return false;
            if (isScreenPrint + isTimezone > 0) countRule56Changed++;

            // Rule 7
            if (countRule56Changed > 1) return false;

            return true;
        }

        private bool CheckRatio(string a, string b)
        {
            // Check ratio
            // /api/ratio
            return true;
        }

        public async Task RatioAsync(string url, string a, string b)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            using (var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, new CancellationToken())
                .ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
