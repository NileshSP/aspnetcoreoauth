using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace aspnetcoreoauth.Models
{
    public class EThorTestEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] HardProperty { get; set; }
    }

    public class EThorTestEntityValidator : AbstractValidator<EThorTestEntity>
    {
        public EThorTestEntityValidator(IApplicationDBContext context)
        {
            int entityId = 0;
            CascadeMode = CascadeMode.StopOnFirstFailure;
            When(entity => {
                entityId = entity.Id; 
                return true; 
            }, () =>
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Name is required")
                    .MustAsync(async (name, cancellation)
                                    => !((await context
                                            .EThorTestEntity
                                            .Where(t => (entityId > 0 && t.Id != entityId && t.Name.ToLower().Trim() == name.ToLower().Trim()) // while updating exiting entity
                                                        || (entityId == 0 && t.Name.ToLower().Trim() == name.ToLower().Trim())) // while adding new entity
                                            .ToAsyncEnumerable().Count()
                                            ) > 0
                                        )
                    ).WithMessage("Name already exits");
            });
        }
    }
}
