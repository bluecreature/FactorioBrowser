namespace FactorioBrowser {

   internal interface IProgressReporter {

      void Warn(string warning);

      void Progress(string status);
   }
}
