open System
open System.IO
open Deedle
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double
open RProvider
open RProvider.``base``
open RProvider.datasets
open RProvider.stats

let solutionDir = Path.GetDirectoryName (__SOURCE_DIRECTORY__)
let (%) dir name = Path.Combine (dir, name)
let (~%) name = solutionDir % name

let trees = Frame.ReadCsv (% "trees.csv")
trees.RenameSeries ("Girth", "DBH")

let n = float trees.RowCount
let D = Series.Log trees?DBH
let H = Series.Log trees?Height
let V = Series.Log trees?Volume
let sum = Series.sum
let a =
   matrix [
      [    n; sum D         ; sum H         ]
      [sum D; sum (D ** 2.0); sum (H * D)   ]
      [sum H; sum (H * D)   ; sum (H ** 2.0)]
   ]
let b =
   vector [
      sum V
      sum (D * V)
      sum (H * V)
   ]
let coef = a.QR().Solve(b)
printfn "log(Volume) = %f + %f log(DBH) + %f log(Height)"
   coef.[0]  // -6.631617
   coef.[1]  //  1.982650
   coef.[2]  //  1.117123

printfn "-----"
   
let formula = "log(Volume) ~ log(DBH) + log(Height)"
using (R.lm (formula, trees)) (R.summary >> R.print)
|> ignore
