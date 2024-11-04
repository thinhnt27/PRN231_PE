
using PRN231.TrialTest.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPRN231_SU24_LeQuyetAnh_Library.Repo;
public class UnitOfWork : IDisposable
{
    private EnglishPremierLeague2024DBContext context = new EnglishPremierLeague2024DBContext();
    private GenericRepo<FootballClub> clubRepo;
    private GenericRepo<PremierLeagueAccount> userAccountRepo;
    private GenericRepo<FootballPlayer> playerRepo;

    public GenericRepo<FootballClub> ClubRepo
    {
        get
        {
            if (clubRepo == null)
            {
                clubRepo = new GenericRepo<FootballClub>(context);
            }
            return clubRepo;
        }
    }

    public GenericRepo<PremierLeagueAccount> UserAccountRepo
    {
        get
        {
            if (userAccountRepo == null)
            {
                userAccountRepo = new GenericRepo<PremierLeagueAccount>(context);
            }
            return userAccountRepo;
        }
    }

    public GenericRepo<FootballPlayer> PlayerRepo
    {
        get
        {
            if (playerRepo == null)
            {
                playerRepo = new GenericRepo<FootballPlayer>(context);
            }
            return playerRepo;
        }
    }

    public async Task SaveAsync()
    {
        await context.SaveChangesAsync();
    }

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}